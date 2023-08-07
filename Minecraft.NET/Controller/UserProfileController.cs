/*
    Minecraft.NET - LFInteractive LLC. 2021-2024﻿
    Minecraft.NET and its libraries are a collection of minecraft related libraries to handle downloading mods, modpacks, resourcepacks, and downloading and installing modloaders (fabric, forge, etc)
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.Minecraft.Model;
using Chase.Networking;
using SkiaSharp;

namespace Chase.Minecraft.Controller;
/// <summary>
/// Controller class for managing user profiles and extracting faces from Minecraft skins.
/// </summary>

public static class UserProfileController
{
    /// <summary>
    /// Retrieves the face of a user profile from the Minecraft skin URL and saves it as a separate
    /// image file.
    /// </summary>
    /// <param name="profile">The user profile containing the skin URL.</param>
    /// <param name="force">Whether to force the re-download of the skin and re-crop the face.</param>
    /// <returns>The file path of the saved face image.</returns>
    public static string GetFace(UserProfile profile, bool force = false)
    {
        string TempPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LFInteractive", "Minecraft.NET")).FullName;
        string profileTexturePath = Path.Combine(TempPath, $"profile-{profile.Id}.png");
        string profileFacePath = Path.Combine(TempPath, $"profile-{profile.Id}-face.png");

        if (force)
        {
            File.Delete(profileTexturePath);
            File.Delete(profileFacePath);
        }

        if (!File.Exists(profileFacePath))
        {
            if (!File.Exists(profileTexturePath))
            {
                using NetworkClient client = new();
                client.DownloadFileAsync(profile.Skins.First().Url, profileTexturePath).Wait();
            }

            int width, height, x, y, overlayX;
            width = height = x = y = 8;
            overlayX = 40;

            using SKBitmap orignalImage = SKBitmap.Decode(profileTexturePath);

            // Check if the overlay is transparent
            using SKBitmap overlayCroppedBitmap = new(width, height);
            SKRectI srcRect = new(overlayX, y, overlayX + width, y + height);
            SKRectI destRect = new(0, 0, width, height);

            using (SKCanvas canvas = new(overlayCroppedBitmap))
            {
                canvas.DrawBitmap(orignalImage, srcRect, destRect);
            }

            // Check if the overlay is transparent
            if (!IsTransparent(overlayCroppedBitmap))
            {
                using SKImage overlayCroppedImage = SKImage.FromBitmap(overlayCroppedBitmap);
                using SKData encodedData = overlayCroppedImage.Encode();
                using FileStream fs = new(profileFacePath, FileMode.Create, FileAccess.Write, FileShare.None);
                encodedData.SaveTo(fs);
            }
            else
            {
                // If the overlay is transparent, get the non-overlay X cropped image
                using SKBitmap nonOverlayCroppedBitmap = new(width, height);
                srcRect = new(x, y, x + width, y + height);

                using (SKCanvas canvas = new(nonOverlayCroppedBitmap))
                {
                    canvas.DrawBitmap(orignalImage, srcRect, destRect);
                }

                if (!IsTransparent(nonOverlayCroppedBitmap))
                {
                    using SKImage nonOverlayCroppedImage = SKImage.FromBitmap(nonOverlayCroppedBitmap);
                    using SKData encodedData = nonOverlayCroppedImage.Encode();
                    using FileStream fs = new(profileFacePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    encodedData.SaveTo(fs);
                }
                // If both the overlay and normal X cropping are transparent, save the original
                // cropped image
                else
                {
                    using SKImage croppedImage = SKImage.FromBitmap(orignalImage);
                    using SKData encodedData = croppedImage.Encode();
                    using FileStream fs = new(profileFacePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    encodedData.SaveTo(fs);
                }
            }
        }

        return profileFacePath;
    }

    /// <summary>
    /// Returns the face image as base64
    /// </summary>
    /// <param name="profile">The user profile containing the skin URL.</param>
    /// <param name="force">Whether to force the re-download of the skin and re-crop the face.</param>
    /// <returns>The base64 of the saved face image.</returns>
    public static string GetFaceBase64(UserProfile profile, bool force = false)
    {
        string path = GetFace(profile, force);

        using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using MemoryStream ms = new();
        fs.CopyTo(ms);
        byte[] bytes = ms.ToArray();
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Checks if an SKBitmap image contains entirely transparent pixels.
    /// </summary>
    /// <param name="bitmap">The SKBitmap image to check.</param>
    /// <returns>True if the image contains entirely transparent pixels; otherwise, false.</returns>
    private static bool IsTransparent(SKBitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor pixelColor = bitmap.GetPixel(x, y);
                if (pixelColor.Alpha > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }
}