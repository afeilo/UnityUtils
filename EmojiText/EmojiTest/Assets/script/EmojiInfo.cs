public class EmojiInfo{
	public char[] regex;
	public string key;
	public float x;
    public float y;
    public float size;
    public EmojiInfo(char[] regex, string key, float x, float y, float size)
    {
		this.regex = regex;
		this.key = key;
		this.x = x;
		this.y = y;
		this.size = size;
	}
}

