using ICities;
using UnityEngine;

using System;
using System.Reflection;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Packaging;

namespace LastModified
{
    public class Mod : IUserMod
    {
        public string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string Name => "Default to Last Modified " + Version;

        public string Description => "Always sort by last modified when saving the game.";
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (Singleton<ToolManager>.exists && Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.Game)
            {
                SavePanel savePanel = UIView.library.Get<SavePanel>("SavePanel");
                UIDropDown sortBy = (UIDropDown) (savePanel.component.Find("SortBy"));
                Debug.Log(sortBy.name);
                sortBy.selectedIndex = 1;

                string lastLoadedName = "";
                DateTime lastLoadedDT = new DateTime();
                foreach (Package.Asset item in PackageManager.FilterAssets(UserAssetType.SaveGameMetaData))
                {
                    if (!PackageHelper.IsDemoModeSave(item) && item != null && item.isEnabled)
                    {
                        try
                        {
                            SaveGameMetaData saveGameMetaData = item.Instantiate<SaveGameMetaData>();
                            if (saveGameMetaData.timeStamp > lastLoadedDT) // more recent than last loaded save
                            {
                                lastLoadedDT = saveGameMetaData.timeStamp;
                                lastLoadedName = item.name;
                            }
                        }
                        catch (Exception ex)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Serialization, "'" + item.name + "' failed to load.\nThis save may be corrupted or may have been removed.\n" + ex.ToString());
                        }
                    }
                }

                if (lastLoadedName == "") return;
                Debug.Log("[Default to Last Modified]: Last loaded save is " + lastLoadedName + ".");
                SavePanel.lastLoadedName = lastLoadedName;
            }
        }
    }
}