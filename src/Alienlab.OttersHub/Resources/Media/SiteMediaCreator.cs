namespace Alienlab.OttersHub.Resources.Media
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Web.Hosting;

  using Sitecore;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.IO;
  using Sitecore.Resources.Media;
  using Sitecore.SecurityModel;
  using Sitecore.Sites;

  public class SiteMediaCreator : MediaCreator
  {
    [NotNull]
    private static readonly Dictionary<string, string> map = SiteManager.GetSites().Where(x => !string.IsNullOrEmpty(x.Properties["mediaRootPath"])).ToDictionary(x => x.Properties["mediaRootPath"], x => x.Properties["mediaFileFolder"]);

    protected override string GetFullFilePath(ID itemID, string fileName, string itemPath, MediaCreatorOptions options)
    {
      Assert.ArgumentNotNull(itemID, nameof(itemID));
      Assert.ArgumentNotNull(fileName, nameof(fileName));
      Assert.ArgumentNotNull(itemPath, nameof(itemPath));
      Assert.ArgumentNotNull(options, nameof(options));

      using (new SecurityDisabler())
      {
        var parentPath = itemPath.Substring(0, itemPath.LastIndexOf('/'));
        var parentId = parentPath.Substring(parentPath.LastIndexOf('/') + 1);
        var database = options.Database ?? Context.ContentDatabase ?? Context.Database;
        if (database == null)
        {
          return base.GetFullFilePath(itemID, fileName, itemPath, options);
        }

        var item = database.GetItem(parentId);
        if (item == null)
        {
          return base.GetFullFilePath(itemID, fileName, itemPath, options);
        }

        var fullPath = item.Paths.FullPath;
        var pair = FindMediaDirectory(fullPath);
        if (pair == null)
        {
          return base.GetFullFilePath(itemID, fileName, itemPath, options);
        }

        var folder = HostingEnvironment.MapPath(Path.Combine(pair.Value.Value, fullPath.Substring(pair.Value.Key.Length).Trim("/\\".ToCharArray())));
        if (!Directory.Exists(folder))
        {
          Directory.CreateDirectory(folder);
        }

        return FileUtil.GetUniqueFilename(Path.Combine(folder, Path.GetFileName(fileName)));
      }
    }

    public static KeyValuePair<string, string>? FindMediaDirectory(string itemPath)
    {
      foreach (var pair in map)
      {
        if (itemPath.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
        {
          return pair;
        }
      }

      return null;
    }
  }
}