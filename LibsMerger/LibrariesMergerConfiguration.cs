using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LibsMerger;

public sealed class LibrariesMergerConfiguration
{
    private const string ConfigurationName = "libraries-merger.config";

    readonly List<string> _toIgnore = new();
    
    public bool Loaded { get; private set; }
    public IReadOnlyList<string> ToIgnore => _toIgnore;
    
    public void Load()
    {
        XmlDocument doc = new XmlDocument();

        try
        {
            doc.Load(ConfigurationName);
            Loaded = true;
        }
        catch (FileNotFoundException e)
        {
            Loaded = false;
        }

        if (!Loaded)
        {
            return;
        }
        
        LoadLibrariesToIgnore(doc);
    }

    private void LoadLibrariesToIgnore(XmlDocument doc)
    {
        XmlNodeList ignoreNodes = doc.SelectNodes("/ignore/entry");

        if (ignoreNodes != null)
        {
            foreach (XmlNode ignoreNode in ignoreNodes)
            {
                if (ignoreNode.Attributes == null)
                {
                    continue;
                }
                
                XmlAttribute nameAttribute = ignoreNode.Attributes["name"];

                if (nameAttribute == null)
                {
                    continue;
                }

                string name = nameAttribute.Value;
                
                _toIgnore.Add(name);
            }
        }
    }
}