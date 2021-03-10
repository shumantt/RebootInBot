namespace RebootInBot.ConsoleBot
{
    public class ConsoleMessage
    {
        public ConsoleMessage(int left, int top, string text)
        {
            Left = left;
            Top = top;
            Text = text;
        }
                
        public int Left { get; }
        public int Top { get; }
        public string Text { get; set; }

        public ConsoleMessage Rewrite(string newText) => new ConsoleMessage(Left, Top, newText);
    }
}