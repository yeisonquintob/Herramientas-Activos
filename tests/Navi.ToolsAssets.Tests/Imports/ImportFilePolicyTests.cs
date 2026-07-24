using Navi.ToolsAssets.Api.Imports;

namespace Navi.ToolsAssets.Tests.Imports;

public sealed class ImportFilePolicyTests
{
    [Theory]
    [InlineData("inventory.xls")]
    [InlineData("inventory.xlsm")]
    [InlineData("inventory.csv")]
    [InlineData("../inventory.exe")]
    public void ValidateMetadata_RejectsUnsafeExtensions(string fileName)
    {
        var result = ImportFilePolicy.ValidateMetadata(
            fileName,
            "application/octet-stream",
            100,
            ImportFilePolicy.DefaultMaximumFileSizeBytes);

        Assert.False(result.IsValid);
        Assert.Contains(".xlsx", result.Error);
    }

    [Fact]
    public void ValidateMetadata_AcceptsBoundedXlsx()
    {
        var result = ImportFilePolicy.ValidateMetadata(
            "inventory.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            1024,
            ImportFilePolicy.DefaultMaximumFileSizeBytes);

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ValidateMetadata_RejectsOversizedFile()
    {
        var result = ImportFilePolicy.ValidateMetadata(
            "inventory.xlsx",
            "application/octet-stream",
            101,
            100);

        Assert.False(result.IsValid);
        Assert.Contains("tamaño máximo", result.Error);
    }

    [Fact]
    public void HasOpenXmlSignature_RecognizesZipAndRestoresPosition()
    {
        using var stream = new MemoryStream(new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x01 });
        stream.Position = 2;

        var result = ImportFilePolicy.HasOpenXmlSignature(stream);

        Assert.True(result);
        Assert.Equal(2, stream.Position);
    }

    [Fact]
    public void HasOpenXmlSignature_RejectsArbitraryContent()
    {
        using var stream = new MemoryStream("not an xlsx"u8.ToArray());

        Assert.False(ImportFilePolicy.HasOpenXmlSignature(stream));
    }
}
