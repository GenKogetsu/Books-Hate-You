
namespace Kogetsu.Library.Database
{
    [CreateAssetMenu(fileName = "ObjectPathsDatabase", menuName = "KogetsuLibrary/Database/ObjectPathsDatabase")]
    public class ObjectPathsDatabaseSO : ScriptableObject
    {
        public List<Object> ObjectPathsDatabase = new();
    }
}

