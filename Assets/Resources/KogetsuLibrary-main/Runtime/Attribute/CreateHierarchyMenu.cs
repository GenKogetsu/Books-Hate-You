using System;

namespace Kogetsu.Library.Attribute
{
    /// <summary>
    /// Marks a MonoBehaviour class so HierarchyMenuGenerator produces
    /// "GameObject / [MenuPath] / [PrefabName]" entries for every prefab in
    /// the project that contains this component.
    ///
    /// The prefab name is discovered automatically — no need to declare it:
    /// <code>
    ///   [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    ///   public class BasicInputManager : MonoBehaviour { }
    /// </code>
    /// If two prefabs share the same component they both appear in the menu,
    /// each under its own prefab name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CreateHierarchyMenu : System.Attribute
    {
        /// <summary>Path after "GameObject/" in the menu, e.g. "KogetsuLibrary/Core/Manager".</summary>
        public string MenuPath { get; }

        public CreateHierarchyMenu(string menuPath) => MenuPath = menuPath;
    }
}
