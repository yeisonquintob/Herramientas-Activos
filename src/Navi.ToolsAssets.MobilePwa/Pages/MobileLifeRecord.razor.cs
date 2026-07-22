using Microsoft.AspNetCore.Components.Forms;
using Navi.ToolsAssets.MobilePwa.Models;

namespace Navi.ToolsAssets.MobilePwa.Pages;

public partial class MobileLifeRecord
{
    private const long MaxLifeImageSizeV45 =
        10 * 1024 * 1024;

    private bool isUploadingLifeImageV45;

    private bool lifeImageMessageIsErrorV45;

    private string? lifeImageMessageV45;


    private bool CanUploadLifeImageV45
    {
        get
        {
            return CanEditTechnical
                   && Auth.HasPermission(
                       "Documents.Upload");
        }
    }


    private bool IsUploadingLifeImageV45
    {
        get
        {
            return isUploadingLifeImageV45;
        }
    }


    private string? LifeImageMessageV45
    {
        get
        {
            return lifeImageMessageV45;
        }
    }


    private string LifeUploadButtonTextV45
    {
        get
        {
            return isUploadingLifeImageV45
                ? "Subiendo..."
                : "Subir imagen";
        }
    }


    private string LifeImageMessageClassV45
    {
        get
        {
            return lifeImageMessageIsErrorV45
                ? "mlife-v45-image-message error"
                : "mlife-v45-image-message success";
        }
    }


    private MobileDocumentDto? LifeImageV45
    {
        get
        {
            return documents
                .Where(IsLifeImageV45)
                .OrderByDescending(
                    item =>
                        item.UploadedAt
                        ?? DateTime.MinValue)
                .FirstOrDefault();
        }
    }


    private string LifeImageUrlV45
    {
        get
        {
            var image = LifeImageV45;

            if (
                image is null
                || string.IsNullOrWhiteSpace(
                    image.DownloadUrl))
            {
                return string.Empty;
            }

            return Api.ToApiUrl(
                image.DownloadUrl);
        }
    }


    private string LifeImageFileNameV45
    {
        get
        {
            return LifeImageV45?.FileName
                   ?? "No hay imagen registrada";
        }
    }


    private static bool IsLifeImageV45(
        MobileDocumentDto item)
    {
        if (
            string.IsNullOrWhiteSpace(
                item.DownloadUrl))
        {
            return false;
        }

        if (
            item.ContentType?.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase)
            == true)
        {
            return true;
        }

        var extension =
            System.IO.Path.GetExtension(
                item.FileName
                ?? string.Empty);

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
                   StringComparison.OrdinalIgnoreCase);
    }


    private async Task OnLifeImageSelectedV45(
        InputFileChangeEventArgs args)
    {
        lifeImageMessageV45 = null;
        lifeImageMessageIsErrorV45 = false;

        if (!CanUploadLifeImageV45)
        {
            lifeImageMessageIsErrorV45 = true;

            lifeImageMessageV45 =
                "No tienes permiso para subir imágenes.";

            return;
        }

        var file = args.File;

        var extension =
            System.IO.Path.GetExtension(
                    file.Name)
                .ToLowerInvariant();

        var validExtension =
            extension == ".jpg"
            || extension == ".jpeg"
            || extension == ".png";

        if (!validExtension)
        {
            lifeImageMessageIsErrorV45 = true;

            lifeImageMessageV45 =
                "Solo se permiten imágenes JPG, JPEG o PNG.";

            return;
        }

        if (file.Size > MaxLifeImageSizeV45)
        {
            lifeImageMessageIsErrorV45 = true;

            lifeImageMessageV45 =
                "La imagen supera el tamaño máximo de 10 MB.";

            return;
        }

        try
        {
            isUploadingLifeImageV45 = true;

            await Api.UploadToolDocumentAsync(
                ToolId,
                file,
                "PhotoEvidence",
                "Imagen principal de la hoja de vida técnica.",
                Auth.CurrentUser?.UserName,
                MaxLifeImageSizeV45);

            await LoadAsync();

            lifeImageMessageV45 =
                "Imagen actualizada correctamente.";
        }
        catch (Exception exception)
        {
            lifeImageMessageIsErrorV45 = true;

            lifeImageMessageV45 =
                "No se pudo subir la imagen: "
                + exception.Message;
        }
        finally
        {
            isUploadingLifeImageV45 = false;
        }
    }

    // NAVI MOBILE LIFE RECORD IMAGE URL V46 START

    private string LifeImageUrlV46
    {
        get
        {
            var image = LifeImageV45;

            if (
                image is null
                ||
                string.IsNullOrWhiteSpace(
                    image.DownloadUrl))
            {
                return string.Empty;
            }

            var rawUrl =
                image.DownloadUrl.Trim();

            if (
                System.Uri.TryCreate(
                    rawUrl,
                    System.UriKind.Absolute,
                    out var absoluteUrl))
            {
                if (!absoluteUrl.IsFile)
                {
                    return absoluteUrl.ToString();
                }

                rawUrl =
                    absoluteUrl.AbsolutePath;
            }

            rawUrl =
                rawUrl.TrimStart('/');

            return Api.ToApiUrl(rawUrl);
        }
    }

    // NAVI MOBILE LIFE RECORD IMAGE URL V46 END

    // NAVI MOBILE LIFE RECORD IMAGE MODAL V49 START

    private bool isLifeImagePreviewOpenV49;

    private bool IsLifeImagePreviewOpenV49
    {
        get
        {
            return isLifeImagePreviewOpenV49;
        }
    }

    private void OpenLifeImagePreviewV49()
    {
        if (
            string.IsNullOrWhiteSpace(
                LifeImageUrlV46))
        {
            return;
        }

        isLifeImagePreviewOpenV49 = true;
    }

    private void CloseLifeImagePreviewV49()
    {
        isLifeImagePreviewOpenV49 = false;
    }

    // NAVI MOBILE LIFE RECORD IMAGE MODAL V49 END



}
