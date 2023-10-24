using System;

[Serializable]
public class ImageVideoContainer
{
    public string VideoPath;
    public string ImagePath;
    public float ImageWidth;

    public ImageVideoContainer(string video, string image, float width)
    {
        VideoPath = video;
        ImagePath = image;
        ImageWidth = width;
    }

}
