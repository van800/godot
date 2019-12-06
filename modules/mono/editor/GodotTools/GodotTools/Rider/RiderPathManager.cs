using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using GodotTools.Internals;

namespace GodotTools.Rider
{
  public static class RiderPathManager
  {
    private static string GetRiderPathFromSettings()
    {
      var editorSettings = GodotSharpEditor.Instance.GetEditorInterface().GetEditorSettings();
      return (string) editorSettings.GetSetting("mono/editor/rider_path");
    }
    
    public static void Initialize()
    {
      var editorSettings = GodotSharpEditor.Instance.GetEditorInterface().GetEditorSettings();
      editorSettings.AddPropertyInfo(new Godot.Collections.Dictionary
      {
        ["type"] = Variant.Type.String,
        ["name"] = "mono/editor/rider_path",
        ["hint"] = PropertyHint.File,
        ["hint_string"] = ""
      });

      var editor = (ExternalEditorId) editorSettings.GetSetting("mono/editor/external_editor");

      if (editor == ExternalEditorId.Rider)
      {
        var riderPath = (string) editorSettings.GetSetting("mono/editor/rider_path");
        if (IsRiderAndExists(riderPath))
        {
          Globals.EditorDef("mono/editor/rider_path", riderPath);
          return;
        }
        
        var paths = RiderPathLocator.GetAllRiderPaths();

        if (!paths.Any())
          return;

        Globals.EditorDef("mono/editor/rider_path", paths.Last().Path);
      }
    }

    private static bool IsRider(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return false;
      }

      var fileInfo = new FileInfo(path);
      var filename = fileInfo.Name.ToLowerInvariant();
      return filename.StartsWith("rider", StringComparison.Ordinal);
    }

    private static string CheckAndUpdatePath(string riderPath)
    {
      if (IsRiderAndExists(riderPath))
      {
        return riderPath;
      }

      var editorSettings = GodotSharpEditor.Instance.GetEditorInterface().GetEditorSettings();
      var paths = RiderPathLocator.GetAllRiderPaths();

      if (!paths.Any())
        return null;
        
      var newPath = paths.Last().Path;
      editorSettings.SetSetting("mono/editor/rider_path", newPath);
      Globals.EditorDef("mono/editor/rider_path", newPath);
      return newPath;
    }

    private static bool IsRiderAndExists(string riderPath)
    {
      return !string.IsNullOrEmpty(riderPath) && IsRider(riderPath) && new FileInfo(riderPath).Exists;
    }

    public static void OpenFile(string slnPath, string scriptPath, int line)
    {
      var pathFromSettings = GetRiderPathFromSettings();
      var path = CheckAndUpdatePath(pathFromSettings);
      
      var args = new List<string>();
      args.Add(slnPath);
      if (line >= 0)
      {
        args.Add("--line");
        args.Add(line.ToString());
      }
      args.Add(scriptPath);
      try
      {
        Utils.OS.RunProcess(path, args);
      }
      catch (Exception e)
      {
        GD.PushError($"Error when trying to run code editor: JetBrains Rider. Exception message: '{e.Message}'");
      }
    }
  }
}