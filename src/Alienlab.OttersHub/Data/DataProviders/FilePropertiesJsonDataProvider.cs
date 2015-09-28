namespace Alienlab.OttersHub.Data.DataProviders
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  using Sitecore;
  using Sitecore.Configuration;
  using Sitecore.Data.DataProviders;
  using Sitecore.Text;

  [UsedImplicitly]
  public class FilePropertiesJsonDataProvider : JsonDataProvider
  {
    protected readonly string PropertiesFolderPath;
    public FilePropertiesJsonDataProvider(string connectionString, string databaseName, string betterMerging)
      : base(connectionString, databaseName, betterMerging)
    {
      var path = MainUtil.MapPath(Path.Combine(Settings.DataFolder, "Properties", databaseName));
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      this.PropertiesFolderPath = path;
    }

    public FilePropertiesJsonDataProvider(string connectionString, string databaseName)
      : base(connectionString, databaseName)
    {
      var path = MainUtil.MapPath(Path.Combine(Settings.DataFolder, "Properties", databaseName));
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      this.PropertiesFolderPath = path;
    }

    public override List<string> GetPropertyKeys(string prefix, CallContext context)
    {
      return Directory.GetFiles(this.PropertiesFolderPath, prefix + "*.txt").ToList();
    }

    protected override string GetPropertyCore(string propertyName, CallContext context)
    {
      var path = GetPropertyFilePath(propertyName);
      if (!File.Exists(path))
      {
        return null;
      }

      return File.ReadAllText(path);
    }

    protected override bool SetPropertyCore(string propertyName, string value, CallContext context)
    {
      lock (this.GetLock(propertyName))
      {
        File.WriteAllText(GetPropertyFilePath(propertyName), value);

        return true;
      }
    }

    protected override bool RemovePropertyCore(string propertyName, bool isPrefix, CallContext context)
    {
      if (isPrefix)
      {
        var files = Directory.GetFiles(this.PropertiesFolderPath, propertyName + "*.txt");
        foreach (var file in files)
        {
          File.Delete(file);
        }
      }
      else
      {
        var path = GetPropertyFilePath(propertyName);
        if (File.Exists(path))
        {
          File.Delete(path);
        }
      }

      return true;
    }

    private string GetPropertyFilePath(string propertyName)
    {
      return Path.Combine(this.PropertiesFolderPath, propertyName + ".txt");
    }
  }
}