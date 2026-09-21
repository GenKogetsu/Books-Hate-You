using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    public class CooldowmContoller : Singleton<CooldowmContoller>
    {
        [SerializeField] protected List<BaseSkillMono> Skills = new();
        protected readonly Dictionary<string, ISkill> SkillMap = new();

        protected override void Awake()
        {
            base.Awake();

            SkillMap.Clear();

            foreach (var skill in Skills)
            {
                if (SkillMap.ContainsKey(skill.Name)) return;
                
                SkillMap.Add(skill.Name, skill);
            }
        }

        protected virtual void Update()
        {
            foreach (var skill in SkillMap.Values)
            {
                skill.Tick(Time.deltaTime);
            }
        }

        protected virtual void AddSkill(ISkill newSkill)
        {
            if (SkillMap.ContainsKey(newSkill.Name)) return;

            SkillMap.Add(newSkill.Name, newSkill);
        }
    }
}