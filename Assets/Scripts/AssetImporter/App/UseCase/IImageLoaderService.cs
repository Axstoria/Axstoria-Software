using UnityEngine;

namespace AssetImporter.AssetImporter.App.UseCase
{
    public interface IImageLoaderService
    {
        Sprite LoadSprite(string fileName);
    }
}