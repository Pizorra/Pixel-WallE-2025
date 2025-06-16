
namespace PixelWallE
{
    public interface IVisualizer
    {
        void ExecuteVisualSpawn(int x, int y);
        void ExecuteVisualColor(Color color);
        void ExecuteVisualBrushSize(int size);
        void ExecuteVisualDrawLine(int startX, int startY, int endX, int endY, Color color, int brushSize);
        void ExecuteVisualDrawCircle(int centerX, int centerY, int radius, Color color, int brushSize);
        void ExecuteVisualDrawRectangle(int x, int y, int width, int height, Color color, int brushSize);
        void ExecuteVisualFill(int x, int y, Color color);
    }
}