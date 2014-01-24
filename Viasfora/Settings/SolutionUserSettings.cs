﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Winterdom.Viasfora.Settings {
  public class SolutionUserSettings : ISolutionUserSettings {
    private String settingsFile;
    public const String FILENAME = "settings.vsfuser";
    public const String DEFAULT_FILE = "__";

    public SolutionUserSettings(String location) {
      String path = Path.GetFullPath(location);
      if ( !String.IsNullOrEmpty(Path.GetExtension(path)) ) {
        path = Path.GetDirectoryName(path);
      }
      this.settingsFile = Path.Combine(path, FILENAME);
    }

    public void Store<T>(string filePath, T settingsObject) where T : ISettingsObject {
      if ( String.IsNullOrEmpty(filePath) ) {
        filePath = DEFAULT_FILE;
      }
      var json = TryRead();
      if ( json == null ) {
        json = new JObject();
      }
      SetEntry(json, filePath, settingsObject);

      using ( var sw = new StreamWriter(this.settingsFile) )
      using ( var jw = new JsonTextWriter(sw) ) {
        jw.Formatting = Formatting.Indented;
        json.WriteTo(jw);
      }
    }

    public T Load<T>(string filePath) where T : ISettingsObject, new() {
      if ( String.IsNullOrEmpty(filePath) ) {
        filePath = DEFAULT_FILE;
      }
      var json = TryRead();
      if ( json == null ) {
        return default(T);
      }
      return GetEntry<T>(json, filePath);
    }


    private T GetEntry<T>(JObject json, String key) where T : ISettingsObject, new() {
      var keyEntry = json[key];
      if ( keyEntry == null ) {
        return default(T);
      }
      T settingsObj = new T();
      var settingsEntry = keyEntry[settingsObj.Name];
      if ( settingsEntry == null ) {
        return default(T);
      }
      DeserializeEntry((JObject)settingsEntry, settingsObj);
      return settingsObj;
    }

    private void DeserializeEntry<T>(JObject settingsEntry, T settingsObj) where T: ISettingsObject {
      String text = settingsEntry.ToString();
      using ( var jr = new JsonTextReader(new StringReader(text)) ) {
        settingsObj.Read(jr);
      }
    }
    private void SetEntry<T>(JObject json, string key, T settingsObject) where T : ISettingsObject {
      var keyEntry = json[key];
      if ( keyEntry == null ) {
        keyEntry = new JObject();
        json[key] = keyEntry;
      }
      var entryObject = SerializeEntry(settingsObject);
      keyEntry[settingsObject.Name] = entryObject;
    }

    private JToken SerializeEntry<T>(T settingsObject) where T : ISettingsObject {
      StringWriter sw = new StringWriter();
      using ( var jw = new JsonTextWriter(sw) ) {
        settingsObject.Save(jw);
      }
      return JObject.Parse(sw.ToString());
    }

    private JObject TryRead() {
      if ( !File.Exists(this.settingsFile) ) {
        return null;
      }
      return JObject.Parse(File.ReadAllText(this.settingsFile));
    }
    private bool SettingsFileExist() {
      return File.Exists(this.settingsFile);
    }
  }
}
