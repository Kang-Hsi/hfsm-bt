using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildBotBT
{
    private const string ROOT_DIR = "Assets/FPS/Scripts/BT/SO";
    private const string TREE_DIR = "Assets/FPS/Scripts/BT/SO/Tree";
    private const string LEAVES_DIR = "Assets/FPS/Scripts/BT/SO/Tree/Leaves";
    private const string HFSM_ACTIONS = "Assets/FPS/Scripts/PracticaIAV/SO/Actions";
    private const string HFSM_CONDITIONS = "Assets/FPS/Scripts/PracticaIAV/SO/Conditions";

    [MenuItem("BT/Build Bot Tree")]
    public static void Build()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Build Bot Tree",
            "This will wipe and rebuild " + TREE_DIR + " from the existing HFSM action / condition assets. Continue?",
            "Build",
            "Cancel");
        if (!confirmed) return;

        if (AssetDatabase.IsValidFolder(TREE_DIR))
            AssetDatabase.DeleteAsset(TREE_DIR);
        AssetDatabase.Refresh();

        AssetDatabase.CreateFolder(ROOT_DIR, "Tree");
        AssetDatabase.CreateFolder(TREE_DIR, "Leaves");

        // Leaves wrapping existing HFSM actions
        var tSelectWeapon = WrapAction("Task_SelectWeapon", "SelectWeapon_Action");
        var tSelectTarget = WrapAction("Task_SelectTarget", "SelectTarget_Action");
        var tFace = WrapAction("Task_Face", "Face_Action");
        var tShoot = WrapAction("Task_Shoot", "Shoot_Action");
        var tFindHealth = WrapAction("Task_FindHealth", "HeatlhPickup_Action");
        var tPickWeapon = WrapAction("Task_PickWeapon", "PickWeapon_Action");
        var tWander = WrapAction("Task_Wander", "Wander_Action");

        // Leaves wrapping existing HFSM conditions
        var tLifeLow = WrapCondition("Task_LifeLow", "Condition_LifeLowerThan");
        var tEnemyNear = WrapCondition("Task_EnemyNear", "Condition_EnemyNear");
        var tEnemyVeryNear = WrapCondition("Task_EnemyVeryNear", "Condition_EnemyVeryNear");
        var tHasAmmo = WrapCondition("Task_HasAmmo", "Condition_Ammo");
        var tNoAllWeapons = WrapCondition("Task_NoHasAllWeapons", "Condition_NoHasAllWeapons");

        // Survive subtree
        var defendSeq = MakeSequence("Seq_Defend",
            tEnemyVeryNear, tSelectWeapon, tSelectTarget, tFace, tShoot);
        var surviveInner = MakeSelector("Sel_SurviveInner",
            defendSeq, tFindHealth);
        var surviveSeq = MakeSequence("Seq_Survive",
            tLifeLow, surviveInner);

        // Combat subtree
        var shootSeq = MakeSequence("Seq_Shoot",
            tHasAmmo, tSelectTarget, tFace, tShoot);
        var combatInner = MakeSelector("Sel_CombatInner",
            shootSeq, tSelectWeapon);
        var combatSeq = MakeSequence("Seq_Combat",
            tEnemyNear, combatInner);

        // Explore subtree
        var findWeaponSeq = MakeSequence("Seq_FindWeapon",
            tNoAllWeapons, tPickWeapon);
        var exploreSel = MakeSelector("Sel_Explore",
            findWeaponSeq, tWander);

        // Root: priority Selector — Survive > Combat > Explore
        var root = MakeSelector("BT_Root",
            surviveSeq, combatSeq, exploreSel);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("BT tree built at " + TREE_DIR + ". Root asset: " + AssetDatabase.GetAssetPath(root));
        EditorGUIUtility.PingObject(root);
        Selection.activeObject = root;
    }

    private static ActionTaskSO WrapAction(string assetName, string hfsmAssetName)
    {
        var src = AssetDatabase.LoadAssetAtPath<ActionSO>($"{HFSM_ACTIONS}/{hfsmAssetName}.asset");
        if (src == null)
        {
            Debug.LogError("BuildBotBT: missing HFSM action asset " + hfsmAssetName);
            return null;
        }
        var wrapper = ScriptableObject.CreateInstance<ActionTaskSO>();
        wrapper.action = src;
        AssetDatabase.CreateAsset(wrapper, $"{LEAVES_DIR}/{assetName}.asset");
        return wrapper;
    }

    private static ConditionTaskSO WrapCondition(string assetName, string hfsmAssetName)
    {
        var src = AssetDatabase.LoadAssetAtPath<ConditionSO>($"{HFSM_CONDITIONS}/{hfsmAssetName}.asset");
        if (src == null)
        {
            Debug.LogError("BuildBotBT: missing HFSM condition asset " + hfsmAssetName);
            return null;
        }
        var wrapper = ScriptableObject.CreateInstance<ConditionTaskSO>();
        wrapper.condition = src;
        AssetDatabase.CreateAsset(wrapper, $"{LEAVES_DIR}/{assetName}.asset");
        return wrapper;
    }

    private static SequenceSO MakeSequence(string assetName, params TaskSO[] children)
    {
        var seq = ScriptableObject.CreateInstance<SequenceSO>();
        seq.children = new List<TaskSO>(children);
        AssetDatabase.CreateAsset(seq, $"{TREE_DIR}/{assetName}.asset");
        return seq;
    }

    private static SelectorSO MakeSelector(string assetName, params TaskSO[] children)
    {
        var sel = ScriptableObject.CreateInstance<SelectorSO>();
        sel.children = new List<TaskSO>(children);
        AssetDatabase.CreateAsset(sel, $"{TREE_DIR}/{assetName}.asset");
        return sel;
    }
}
