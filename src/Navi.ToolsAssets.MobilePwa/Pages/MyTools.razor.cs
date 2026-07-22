using System;
using System.Linq;
using System.Threading.Tasks;
using Navi.ToolsAssets.MobilePwa.Models;

namespace Navi.ToolsAssets.MobilePwa.Pages;

public partial class MyTools
{
    // NAVI MOBILE INVENTORY IMAGE PREVIEW V54 START

    private MobileToolDto? inventoryPreviewToolV54;

    private bool isInventoryImagePreviewOpenV54;

    private bool isInventoryImageLoadingV54;

    private string? inventoryImageUrlV54;

    private string? inventoryImageErrorV54;


    private MobileToolDto? InventoryPreviewToolV54
    {
        get
        {
            return inventoryPreviewToolV54;
        }
    }


    private bool IsInventoryImagePreviewOpenV54
    {
        get
        {
            return isInventoryImagePreviewOpenV54;
        }
    }


    private bool IsInventoryImageLoadingV54
    {
        get
        {
            return isInventoryImageLoadingV54;
        }
    }


    private string? InventoryImageUrlV54
    {
        get
        {
            return inventoryImageUrlV54;
        }
    }


    private string? InventoryImageErrorV54
    {
        get
        {
            return inventoryImageErrorV54;
        }
    }


    private async Task OpenInventoryImagePreviewV54(
        MobileToolDto tool)
    {
        inventoryPreviewToolV54 = tool;

        isInventoryImagePreviewOpenV54 = true;
        isInventoryImageLoadingV54 = true;

        inventoryImageUrlV54 = null;
        inventoryImageErrorV54 = null;

        try
        {
            var documents =
                await Api.GetListJsonAsync<
                    MobileDocumentDto>(
                    $"api/tools/{tool.Id}/documents");

            var latestImage =
                documents
                    .Where(
                        IsInventoryImageDocumentV54)
                    .OrderByDescending(
                        document =>
                            document.UploadedAt)
                    .FirstOrDefault();

            if (
                latestImage is null
                ||
                string.IsNullOrWhiteSpace(
                    latestImage.DownloadUrl))
            {
                inventoryImageErrorV54 =
                    "Sin imagen registrada";

                return;
            }

            inventoryImageUrlV54 =
                NormalizeInventoryImageUrlV54(
                    latestImage.DownloadUrl);
        }
        catch (Exception exception)
        {
            inventoryImageErrorV54 =
                "No se pudo cargar la imagen.";

            System.Diagnostics.Debug.WriteLine(
                exception);
        }
        finally
        {
            isInventoryImageLoadingV54 = false;
        }
    }


    private void CloseInventoryImagePreviewV54()
    {
        isInventoryImagePreviewOpenV54 = false;
        isInventoryImageLoadingV54 = false;

        inventoryPreviewToolV54 = null;

        inventoryImageUrlV54 = null;
        inventoryImageErrorV54 = null;
    }


    private static bool IsInventoryImageDocumentV54(
        MobileDocumentDto document)
    {
        if (
            document.ContentType?.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase)
            == true)
        {
            return true;
        }

        var extension =
            System.IO.Path.GetExtension(
                document.FileName
                ??
                string.Empty);

        return extension.Equals(
                   ".jpg",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".jpeg",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".png",
                   StringComparison.OrdinalIgnoreCase)
               ||
               extension.Equals(
                   ".webp",
                   StringComparison.OrdinalIgnoreCase);
    }


    private string NormalizeInventoryImageUrlV54(
        string downloadUrl)
    {
        var cleanUrl =
            downloadUrl.Trim();

        if (
            cleanUrl.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase)
            ||
            cleanUrl.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase))
        {
            return cleanUrl;
        }

        if (
            cleanUrl.StartsWith(
                "file://",
                StringComparison.OrdinalIgnoreCase)
            &&
            Uri.TryCreate(
                cleanUrl,
                UriKind.Absolute,
                out var fileUrl))
        {
            cleanUrl =
                fileUrl.AbsolutePath;
        }

        cleanUrl =
            cleanUrl.TrimStart('/');

        return Api.ToApiUrl(
            cleanUrl);
    }

    // NAVI MOBILE INVENTORY IMAGE PREVIEW V54 END

}
