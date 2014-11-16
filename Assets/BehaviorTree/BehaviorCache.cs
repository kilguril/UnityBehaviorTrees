using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;

namespace BehaviorTrees
{
    public static class BehaviorCache
    {
        private static Dictionary< string, BehaviorTree > cache;


        static BehaviorCache()
        {
            cache = new Dictionary<string, BehaviorTree>();
            GetGeneratorsFromAssembly();
        }


        public static void Register( string id, BehaviorTree behavior )
        {
            if ( cache.ContainsKey( id ) )
            {
                Debug.LogWarning( string.Format( "Trying to register duplicate behavior tree entry `{0}`", id ) );
                return;
            }

            cache[ id ] = behavior;
            Debug.Log( string.Format( "Registered behavior `{0}`", id ) );
        }


        public static BehaviorTree GetBehavior( string id )
        {
            if ( cache.ContainsKey( id ) )
            {
                return cache[ id ];
            }

            Debug.LogWarning( string.Format( "Trying to retrieve an undefined behavior tree `{0}`", id ) );
            return null;
        }


        private static void GetGeneratorsFromAssembly()
        {
            Type t = typeof( BehaviorCache );
            Assembly asm = t.Assembly;

            Type[] types = asm.GetTypes();

            foreach( Type type in types )
            {
                MethodInfo[] methods = type.GetMethods( BindingFlags.Public | BindingFlags.Static );
                
                foreach( MethodInfo method in methods )
                {
                    object[] attr = method.GetCustomAttributes( typeof( BehaviorGeneratorAttribute ), false );
                    
                    if ( attr.Length > 0 )
                    {
                        BehaviorGeneratorAttribute genAttr = ( BehaviorGeneratorAttribute )attr[ 0 ];

                        string          id       = genAttr.id;
                        BehaviorTree    behavior = (BehaviorTree)method.Invoke( null, null );

                        if ( behavior == null )
                        {
                            Debug.LogWarning( string.Format( "Could not invoke generator for behavior `{0}`, is it properly defined?", id ) );
                            continue;
                        }

                        Register( id, behavior );
                    }
                }
            }
        }
    }
}