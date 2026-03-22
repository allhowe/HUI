using System;
using System.IO;
using UIModule;
using UnityEditor;
using UnityEngine;
using ObjectBinderEditor;

public class UIBuildRule : IBuildRule
{
    private UniversalBuildRule universal = new UniversalBuildRule();

    public int Priority => 100;

    public TextAsset Bind(GameObject target)
    {
        if (target.TryGetComponent<BaseView>(out _))
        {
            var scriptPath = UIValidator.GetScriptPath(target.name);
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (monoScript != null)
            {
                return monoScript;
            }
        }
        return null;
    }
    public bool IsValid(TextAsset asset)
    {
        if (asset == null)
        {
            return false;
        }

        var monoScript = asset as MonoScript;

        if (monoScript == null)
        {
            return false;
        }

        Type scriptType = monoScript.GetClass();

        if (scriptType == null)
        {
            return false;
        }
        if (!scriptType.IsSubclassOf(typeof(BaseUI)))
        {
            return false;
        }

        return universal.IsValid(asset);
    }
    public void Build(ObjectBinder binder)
    {
        var scriptPath = AssetDatabase.GetAssetPath(binder.asset);
        var className = ((MonoScript)binder.asset).GetClass().Name;

        var originContent = File.ReadAllText(scriptPath);

        var content = universal.BuildContent(binder, className, originContent);

        content = ReplaceContent(content);

        ObjectBinderHelper.WriteCode(content, scriptPath);
    }

    public string ReplaceContent(string content)
    {
        content = content.Replace("public void InitBind(GameObject target)", "public void InitBind()");
        content = content.Replace($"target.GetComponent<{nameof(ObjectBinder)}>();", $"View.GetComponent<{nameof(ObjectBinder)}>();");
        return content;
    }
}