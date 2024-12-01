using System.Collections;

namespace FCP.Core;

[StaticConstructorOnStartup]
public static class CharacterRoleUtils
{
    // Central dictionary for CharacterDefs grouped by role type
    private static readonly Dictionary<Type, IList> RoleRegistry = new Dictionary<Type, IList>();

    /// <summary>
    /// Helper method to get all CharacterDefs with a specific role
    /// </summary>
    /// <typeparam name="TRole">The CharacterRole you're looking for</typeparam>
    public static IReadOnlyList<CharacterDefWithRole<TRole>> GetAllWithRole<TRole>() where TRole : CharacterRole
    {
        if (RoleRegistry.TryGetValue(typeof(TRole), out IList roleList))
        {
            return (List<CharacterDefWithRole<TRole>>)roleList;
        }

        FCPLog.Error("Failed to retrieve a CharacterDefWithRole<TRole> list from the role registry, returning empty.");
        return new List<CharacterDefWithRole<TRole>>();
    }
    
    /// <summary>
    /// Return a list of all role types in the registry.
    /// </summary>
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

                // Add the role to the registry
                if (!RoleRegistry.ContainsKey(roleType))
                {
                    // List<CharacterDefWithRole<roleType>>
                    Type listType = typeof(List<>).MakeGenericType(typeof(CharacterDefWithRole<>).MakeGenericType(roleType));
                    
                    RoleRegistry[roleType] = (IList)Activator.CreateInstance(listType);
                }

                // Add the entry to the registry
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