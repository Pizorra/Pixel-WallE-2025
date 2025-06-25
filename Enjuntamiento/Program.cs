using System;
using System.Drawing;
using System.Windows.Forms;

namespace PixelWallE
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }


    public partial class MainForm : Form, IVisualizer
    {
        // Matriz que representa el estado del canvas
        private System.Drawing.Color[,] canvas;

        // Tamaño actual del canvas (n x n)
        private int canvasSize;

        // Estado actual del pincel
        private System.Drawing.Color currentBrushColor = System.Drawing.Color.White;
        private int currentBrushSize = 1;

        // Layout principal: dos columnas (izquierda: editor+errores, derecha: canvas)
        private TableLayoutPanel mainLayout;
        // Layout interno en el panel izquierdo (editor y errores)
        private TableLayoutPanel leftLayout;

        // Área de edición: contenedor con números de línea y RichTextBox
        private Panel codeEditorContainer;
        private Panel lineNumbersPanel;
        private RichTextBox codeEditor;

        // Panel de errores
        private RichTextBox errorPanel;

        // Canvas para el grid
        private Panel canvasPanel;

        // Panel de botones inferior
        private Panel buttonsPanel;
        private Button btnCompile;
        private Button btnUpdateGrid;
        private Button btnSave;
        private Button btnLoad;
        private NumericUpDown numericGridSize;

        // Número de celdas del grid (filas y columnas)
        private int gridResolution = 100;
        private int spawnX = -1;
        private int spawnY = -1;
        private Image walleImage;


        public void ExecuteVisualSpawn(int x, int y)
        {
            // No necesita dibujo inmediato, solo actualiza posición
            spawnX = x;
            spawnY = y;
            canvasPanel.Invalidate();
        }

        public void ExecuteVisualColor(Color color)
        {
            currentBrushColor = MapColor(color);
        }

        public void ExecuteVisualBrushSize(int size)
        {
            currentBrushSize = size;
        }

        private void DrawBrushPoint(int x, int y, System.Drawing.Color color, int size)
        {
            if (color == System.Drawing.Color.Transparent)
                return;

            int r = size / 2;
            for (int i = x - r; i <= x + r; i++)
            {
                for (int j = y - r; j <= y + r; j++)
                {
                    if (i >= 0 && i < canvasSize && j >= 0 && j < canvasSize)
                    {
                        // Cambiar: canvas[j, i] en lugar de canvas[i, j]
                        canvas[j, i] = color;
                    }
                }
            }
        }

        public void ExecuteVisualDrawLine(int startX, int startY, int endX, int endY, Color color, int brushSize)
        {
            System.Drawing.Color drawcolor = MapColor(color);
            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);
            int sx = startX < endX ? 1 : -1;
            int sy = startY < endY ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawBrushPoint(startX, startY, drawcolor, brushSize);

                if (startX == endX && startY == endY) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; startX += sx; }
                if (e2 < dx) { err += dx; startY += sy; }
            }
            canvasPanel.Invalidate();
        }

        public void ExecuteVisualDrawCircle(int centerX, int centerY, int radius, Color color, int brushSize)
        {
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                // Dibujar los 8 octantes del círculo
                DrawBrushPoint(centerX + x, centerY + y, MapColor(color), brushSize);
                DrawBrushPoint(centerX + y, centerY + x, MapColor(color), brushSize);
                DrawBrushPoint(centerX - y, centerY + x, MapColor(color), brushSize);
                DrawBrushPoint(centerX - x, centerY + y, MapColor(color), brushSize);
                DrawBrushPoint(centerX - x, centerY - y, MapColor(color), brushSize);
                DrawBrushPoint(centerX - y, centerY - x, MapColor(color), brushSize);
                DrawBrushPoint(centerX + y, centerY - x, MapColor(color), brushSize);
                DrawBrushPoint(centerX + x, centerY - y, MapColor(color), brushSize);

                // Actualizar posición
                if (err <= 0)
                {
                    y++;
                    err += 2 * y + 1;
                }
                if (err > 0)
                {
                    x--;
                    err -= 2 * x + 1;
                }
            }
            canvasPanel.Invalidate();
        }

        public void ExecuteVisualDrawRectangle(int x, int y, int width, int height, Color color, int brushSize)
        {

            ExecuteVisualDrawLine(x, y, x + width - 1, y, color, brushSize); // Top
            ExecuteVisualDrawLine(x + width - 1, y, x + width - 1, y + height - 1, color, brushSize); // Right
            ExecuteVisualDrawLine(x, y + height - 1, x + width - 1, y + height - 1, color, brushSize); // Bottom
            ExecuteVisualDrawLine(x, y, x, y + height - 1, color, brushSize); // Left
            canvasPanel.Invalidate();
        }
        public void ExecuteVisualFill(int x, int y, Color color)
        {
            System.Drawing.Color mycolor = MapColor(color);
            if (mycolor == System.Drawing.Color.Transparent)
                return;
            System.Drawing.Color targetColor = canvas[x, y];
            if (targetColor == mycolor) return;

            Stack<Point> points = new Stack<Point>();
            points.Push(new Point(x, y));

            while (points.Count > 0)
            {
                Point p = points.Pop();

                // Verificar límites
                if (p.X < 0 || p.X >= canvasSize || p.Y < 0 || p.Y >= canvasSize)
                    continue;

                // Verificar si necesita rellenar
                if (canvas[p.X, p.Y] != targetColor)
                    continue;

                // Aplicar nuevo color
                canvas[p.X, p.Y] = mycolor;

                // Agregar vecinos
                points.Push(new Point(p.X + 1, p.Y));
                points.Push(new Point(p.X - 1, p.Y));
                points.Push(new Point(p.X, p.Y + 1));
                points.Push(new Point(p.X, p.Y - 1));
            }
            canvasPanel.Invalidate();
        }
        private System.Drawing.Color MapColor(Color color)
        {
            switch (color)
            {
                case Color.Red: return System.Drawing.Color.Red;
                case Color.Blue: return System.Drawing.Color.Blue;
                case Color.Green: return System.Drawing.Color.Green;
                case Color.Yellow: return System.Drawing.Color.Yellow;
                case Color.Orange: return System.Drawing.Color.Orange;
                case Color.Purple: return System.Drawing.Color.Purple;
                case Color.Black: return System.Drawing.Color.Black;
                case Color.White: return System.Drawing.Color.White;
                case Color.Transparent: return System.Drawing.Color.Transparent;
                default: return System.Drawing.Color.White;
            }
        }
        public MainForm()
        {
            InitializeComponents();
            // Inicializar canvas
            try
            {
                walleImage = Image.FromFile("walle.png"); // Asegúrate de tener el archivo en la carpeta de ejecución
            }
            catch
            {
                walleImage = null; // Manejar errores si la imagen no se carga
            }

            canvasSize = gridResolution;
            canvas = new System.Drawing.Color[canvasSize, canvasSize];
            ClearCanvas();
        }

        private void ClearCanvas()
        {
            for (int i = 0; i < canvasSize; i++)
            {
                for (int j = 0; j < canvasSize; j++)
                {
                    canvas[i, j] = System.Drawing.Color.Transparent;
                }
            }
        }

        private void InitializeComponents()
        {
            this.Text = "Simulador de Compilador";
            this.MinimumSize = new Size(800, 600);
            this.WindowState = FormWindowState.Maximized;

            // ===============================================================
            // Layout principal: dos columnas
            // ===============================================================
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
            };

            // Configuramos la columna izquierda (40%) y la derecha (60%)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // Izquierda
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // Derecha

            // ========================================================
            // Área izquierda: editor y panel de errores
            // ========================================================
            leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            // Ajustamos el editor al 80% de la altura y errores al 20%
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            // --- Editor de código con números de línea ---
            codeEditorContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Panel lateral para los números de línea (fijo 40 pixeles de ancho)
            lineNumbersPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 40,
                BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
            };
            lineNumbersPanel.Paint += LineNumbersPanel_Paint;

            // RichTextBox para edición de código
            codeEditor = new RichTextBox
            {
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both
            };
            // Repintar la numeración en scroll o cambios de texto
            codeEditor.VScroll += (s, e) => lineNumbersPanel.Invalidate();
            codeEditor.TextChanged += (s, e) => lineNumbersPanel.Invalidate();

            codeEditorContainer.Controls.Add(codeEditor);
            codeEditorContainer.Controls.Add(lineNumbersPanel);

            // --- Panel de errores ---
            errorPanel = new RichTextBox
            {
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                ReadOnly = true,
                BackColor = System.Drawing.Color.FloralWhite,
                Text = "Los errores de compilación se mostrarán aquí..."
            };

            // Agregar editor y errores al layout izquierdo
            leftLayout.Controls.Add(codeEditorContainer, 0, 0);
            leftLayout.Controls.Add(errorPanel, 0, 1);

            // ========================================================
            // Área derecha: canvas para el grid
            // ========================================================
            canvasPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None
            };
            canvasPanel.Paint += CanvasPanel_Paint;

            // ========================================================
            // Agregar ambos paneles al layout principal
            // ========================================================
            mainLayout.Controls.Add(leftLayout, 0, 0);
            mainLayout.Controls.Add(canvasPanel, 1, 0);

            // ========================================================
            // Panel inferior: botones
            // ========================================================
            buttonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            btnCompile = new Button
            {
                Text = "Compilar",
                Location = new Point(10, 15),
                Size = new Size(80, 25)
            };
            btnCompile.Click += BtnCompile_Click;

            btnUpdateGrid = new Button
            {
                Text = "Actualizar Grid",
                Location = new Point(100, 15),
                Size = new Size(100, 25)
            };
            btnUpdateGrid.Click += BtnUpdateGrid_Click;

            numericGridSize = new NumericUpDown
            {
                Minimum = 16,
                Maximum = 512,
                Value = gridResolution,
                Increment = 16,
                Width = 60,
                Location = new Point(210, 15),
                Height = 25
            };

            // Nuevos botones para guardar y cargar
            btnSave = new Button
            {
                Text = "Guardar (.pw)",
                Location = new Point(280, 15),
                Size = new Size(100, 25)
            };
            btnSave.Click += BtnSave_Click;

            btnLoad = new Button
            {
                Text = "Cargar (.pw)",
                Location = new Point(390, 15),
                Size = new Size(100, 25)
            };
            btnLoad.Click += BtnLoad_Click;

            buttonsPanel.Controls.Add(btnCompile);
            buttonsPanel.Controls.Add(btnUpdateGrid);
            buttonsPanel.Controls.Add(numericGridSize);
            buttonsPanel.Controls.Add(btnSave);
            buttonsPanel.Controls.Add(btnLoad);

            // ========================================================
            // Agregar controles al formulario
            // ========================================================
            this.Controls.Add(mainLayout);
            this.Controls.Add(buttonsPanel);
        }


        private void LineNumbersPanel_Paint(object sender, PaintEventArgs e)
        {
            int firstCharIndex = codeEditor.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = codeEditor.GetLineFromCharIndex(firstCharIndex);
            Point firstLinePos = codeEditor.GetPositionFromCharIndex(firstCharIndex);
            int lineHeight = TextRenderer.MeasureText("A", codeEditor.Font).Height;
            int visibleLines = codeEditor.ClientSize.Height / lineHeight + 1;

            for (int i = 0; i < visibleLines; i++)
            {
                int lineNumber = firstLine + i + 1;
                float y = firstLinePos.Y + i * lineHeight;
                e.Graphics.DrawString(lineNumber.ToString(), codeEditor.Font, Brushes.Gray, new PointF(0, y));
            }
        }



        //este es el nuevo
        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            // Limpiar fondo
            e.Graphics.Clear(System.Drawing.Color.White);

            // Calcular dimensiones de celda
            int cellWidth = canvasPanel.Width / gridResolution;
            int cellHeight = canvasPanel.Height / gridResolution;

            // Calcular área efectiva de la cuadrícula
            int gridWidth = cellWidth * gridResolution;
            int gridHeight = cellHeight * gridResolution;

            // Calcular offsets para centrar
            int offsetX = (canvasPanel.Width - gridWidth) / 2;
            int offsetY = (canvasPanel.Height - gridHeight) / 2;

            int drawX = offsetX + spawnY * cellWidth;
            int drawY = offsetY + spawnX * cellHeight;

            // Calcular tamaño manteniendo relación de aspecto
            float scale = Math.Min(
                (float)cellWidth / walleImage.Width,
                (float)cellHeight / walleImage.Height
            );

            int scaledWidth = (int)(walleImage.Width * scale);
            int scaledHeight = (int)(walleImage.Height * scale);

            // Centrar la imagen en la celda
            int centeredX = drawX + (cellWidth - scaledWidth) / 2;
            int centeredY = drawY + (cellHeight - scaledHeight) / 2;

            e.Graphics.DrawImage(walleImage, centeredX, centeredY, scaledWidth, scaledHeight);

            // Dibujar píxeles centrados
            for (int x = 0; x < canvasSize; x++)
            {
                for (int y = 0; y < canvasSize; y++)
                {
                    if (canvas[x, y] != System.Drawing.Color.Transparent)
                    {
                        using (SolidBrush brush = new SolidBrush(canvas[x, y]))
                        {
                            // Aplicar offset de centrado a las coordenadas
                            drawX = offsetX + y * cellWidth;
                            drawY = offsetY + x * cellHeight;

                            e.Graphics.FillRectangle(brush, drawX, drawY, cellWidth, cellHeight);
                        }
                    }
                }
            }

            // Dibujar cuadrícula encima de los píxeles (centrada)
            Pen gridPen = Pens.LightGray;

            // Líneas verticales
            for (int i = 0; i <= gridResolution; i++)
            {
                int x = offsetX + i * cellWidth;
                e.Graphics.DrawLine(gridPen, x, offsetY, x, offsetY + gridHeight);
            }

            // Líneas horizontales
            for (int j = 0; j <= gridResolution; j++)
            {
                int y = offsetY + j * cellHeight;
                e.Graphics.DrawLine(gridPen, offsetX, y, offsetX + gridWidth, y);
            }
        }


        private void BtnCompile_Click(object sender, EventArgs e)
        {
            try
            {
                ClearCanvas();
                spawnX = -1; // Resetear posición de spawn
                spawnY = -1;
                string fullCode = codeEditor.Text + Environment.NewLine;
                // Inicializar canvas
                canvasSize = gridResolution;
                canvas = new System.Drawing.Color[canvasSize, canvasSize];

                // Inicializar canvas a blanco
                for (int i = 0; i < canvasSize; i++)
                {
                    for (int j = 0; j < canvasSize; j++)
                    {
                        canvas[i, j] = System.Drawing.Color.White;
                    }
                }

                // Crear intérprete con visualizador
                Interpreter interpreter = new Interpreter(this);

                // Parsear y ejecutar código
                var lexer = new LexicalAnalyzer(fullCode);
                var tokens = lexer.Analyze();
                var parser = new Parser(tokens);
                ProgramNode program = parser.Parse();

                interpreter.Execute(program);

                errorPanel.Text = "Ejecución completada con éxito";
            }
            catch (Exception ex)
            {
                errorPanel.Text = $"Error: {ex.Message}";
            }
        }


        private void BtnUpdateGrid_Click(object sender, EventArgs e)
        {
            gridResolution = (int)numericGridSize.Value;
            spawnX = -1; // Resetear posición de spawn
            spawnY = -1;
            canvas = new System.Drawing.Color[gridResolution, gridResolution];

            // Inicializar a blanco
            for (int i = 0; i < gridResolution; i++)
            {
                for (int j = 0; j < gridResolution; j++)
                {
                    canvas[i, j] = System.Drawing.Color.White;
                }
            }

            canvasPanel.Invalidate();
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Archivos PixelWallE (*.pw)|*.pw";
                saveDialog.DefaultExt = "pw";
                saveDialog.AddExtension = true;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, codeEditor.Text);
                    MessageBox.Show("Archivo guardado exitosamente!", "Guardar",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Archivos PixelWallE (*.pw)|*.pw";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        codeEditor.Text = File.ReadAllText(openDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            walleImage?.Dispose();
        }
    }
}
