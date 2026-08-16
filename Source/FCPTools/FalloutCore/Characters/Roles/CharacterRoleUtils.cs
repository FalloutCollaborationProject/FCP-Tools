using System.Collections;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class CharacterRoleUtils
{
    private static readonly Dictionary<Type, IList> RoleRegistry = new Dictionary<Type, IList>();

    public static IReadOnlyList<CharacterDefWithRole<TRole>> GetAllWithRole<TRole>() where TRole : CharacterRole
    {
        if (RoleRegistry.TryGetValue(typeof(TRole), out IList roleList))
        {
            return (List<CharacterDefWithRole<TRole>>)roleList;
        }

        return new List<CharacterDefWithRole<TRole>>();
    }
    
    public static IReadOnlyList<Type> GetAllRoleTypes()
    {
        return RoleRegistry.Keys.ToList(); 
    }
    
    static CharacterRoleUtils()
    {
        foreach (CharacterDef characterDef in DefDatabase<CharacterDef>.AllDefsListForReading)
        {
            foreach (CharacterRole role in characterDef.roles)
            {
                Type roleType = role.GetType();

                if (!RoleRegistry.ContainsKey(roleType))
                {
                    Type listType = typeof(List<>).MakeGenericType(typeof(CharacterDefWithRole<>).MakeGenericType(roleType));
                    
                    RoleRegistry[roleType] = (IList)Activator.CreateInstance(listType);
                }

                var defWithRole = Activator.CreateInstance(
                    typeof(CharacterDefWithRole<>).MakeGenericType(roleType),
                    characterDef,
                    role
                );
                RoleRegistry[roleType].Add(defWithRole);
            }
        }
    }

    

}