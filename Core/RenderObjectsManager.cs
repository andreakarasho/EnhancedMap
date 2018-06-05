using EnhancedMap.Core.MapObjects;
using EnhancedMap.Core.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedMap.Core
{
    public enum LAYER_ORDER
    {
        BACKGROUND,
        HOUSES,
        BUILDING,
        MOBILES,

        OTHERS,


        USERS,
        LABELS,
        SIGNALS,
        MASK,

        First = BACKGROUND,
        Last = MASK,
    }

    public static class RenderObjectsManager
    {
        private static List<IRenderObject>[] _objects = new List<IRenderObject>[(int)LAYER_ORDER.Last + 1]
        {
            new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>(), new List<IRenderObject>()
        };

        static RenderObjectsManager()
        {
            SocketClient.Disconnected += (sender, e) =>
            {           
                UserObject[] users = Get<UserObject>();
                for (int i = 0; i < users.Length; i++)
                {
                    UserObject user = users[i];
                    if (user is PlayerObject)
                        continue;
                    user.Dispose();
                }           
            };
        }


        public static void AddMap(RenderMap map)
        {
            _objects[(int)LAYER_ORDER.BACKGROUND].Add(map);
        }

        public static void AddUser(UserObject obj)
        {
            _objects[(int)LAYER_ORDER.USERS].Add(obj);
        }

        public static void AddMobile(MobileObject obj)
        {
            _objects[(int)LAYER_ORDER.MOBILES].Add(obj);
        }

        public static void AddBuilding(BuildingObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Add(obj);
        }

        public static void AddHouse(HouseObject obj)
        {
            _objects[(int)LAYER_ORDER.HOUSES].Add(obj);
        }

        public static void AddSharedLabel(SharedLabelObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Add(obj);
        }

        public static void AddGuardline(GuardLineObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Add(obj);
        }

        public static void AddDeathObject(DeathObject obj)
        {
            _objects[(int)LAYER_ORDER.OTHERS].Add(obj);
        }

        public static void AddMarkerObject(MarkerObject obj)
        {
            _objects[(int)LAYER_ORDER.OTHERS].Add(obj);
        }
        public static void AddLabel(LabelObject obj)
        {
            _objects[(int)LAYER_ORDER.LABELS].Add(obj);
        }

        public static void AddSignal(SignalObject obj)
        {
            _objects[(int)LAYER_ORDER.SIGNALS].Add(obj);
        }

        public static void AddMask(RenderObject obj)
        {
            _objects[(int)LAYER_ORDER.MASK].Add(obj);
        }




        public static void RemoveMap(RenderMap map)
        {
            _objects[(int)LAYER_ORDER.BACKGROUND].Remove(map);
        }

        public static void RemoveUser(UserObject obj)
        {
            _objects[(int)LAYER_ORDER.USERS].Remove(obj);
        }

        public static void RemoveMobile(MobileObject obj)
        {
            _objects[(int)LAYER_ORDER.MOBILES].Remove(obj);
        }

        public static void RemoveBuilding(BuildingObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Remove(obj);
        }

        public static void RemoveHouse(HouseObject obj)
        {
            _objects[(int)LAYER_ORDER.HOUSES].Remove(obj);
        }

        public static void RemoveSharedLabel(SharedLabelObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Remove(obj);
        }

        public static void RemoveGuardline(GuardLineObject obj)
        {
            _objects[(int)LAYER_ORDER.BUILDING].Remove(obj);
        }

        public static void RemoveLabel(LabelObject obj)
        {
            _objects[(int)LAYER_ORDER.LABELS].Remove(obj);
        }

        public static void RemoveDeathObject(DeathObject obj)
        {
            _objects[(int)LAYER_ORDER.OTHERS].Remove(obj);
        }

        public static void RemoveMarkerObject(MarkerObject obj)
        {
            _objects[(int)LAYER_ORDER.OTHERS].Remove(obj);
        }

        public static void RemoveSignal(SignalObject obj)
        {
            _objects[(int)LAYER_ORDER.SIGNALS].Remove(obj);
        }

        public static void RemoveMask(RenderObject obj)
        {
            _objects[(int)LAYER_ORDER.MASK].Remove(obj);
        }

        public static UserObject GetUser(string name)
        {
            for (int i = 0; i < _objects[(int)LAYER_ORDER.USERS].Count; i++)
            {
                UserObject user = (UserObject)_objects[(int)LAYER_ORDER.USERS][i];
                if (user != null && !user.IsDisposing && user.Name == name)
                    return user;
            }
            return null;
        }

        public static T[] Get<T>() where T : IRenderObject
        {

           /* var list = (from x in _objects
                        from l in x
                        where l.GetType() == typeof(T)
                        select l)
                        .Cast<T>()
                        .ToArray();

            return list;*/
                    

            if (typeof(T) == typeof(PlayerObject)
                || typeof(T) == typeof(UserObject))
            {
                return _objects[(int)LAYER_ORDER.USERS].Where(s => s is T).Cast<T>().ToArray();
            }
            else if (typeof(T) == typeof(MobileObject))
                return _objects[(int)LAYER_ORDER.MOBILES].Where(s => s is T).Cast<T>().ToArray();
            else if (typeof(T) == typeof(RenderMap))
            {
                return _objects[(int)LAYER_ORDER.BACKGROUND].Where(s => s is RenderMap).Cast<T>().ToArray();
            }
            else if (typeof(T) == typeof(BuildingObject)            
                || typeof(T) == typeof(GuardLineObject) 
                || typeof(T) == typeof(SharedLabelObject))
            {
                return _objects[(int)LAYER_ORDER.BUILDING].Where(s => s is T).Cast<T>().ToArray();
            }
            else if (typeof(T) == typeof(HouseObject))
                return _objects[(int)LAYER_ORDER.HOUSES].Where(s => s is T).Cast<T>().ToArray();
            else if (typeof(T) == typeof(LabelObject))
                return _objects[(int)LAYER_ORDER.LABELS].Where(s => s is T).Cast<T>().ToArray();
            if (typeof(T) == typeof(DeathObject)
                || typeof(T) == typeof(MarkerObject))
                return _objects[(int)LAYER_ORDER.OTHERS].Where(s => s is T).Cast<T>().ToArray();
            else if (typeof(T) == typeof(SignalObject))
                return _objects[(int)LAYER_ORDER.SIGNALS].Where(s => s is T).Cast<T>().ToArray();
            else if (typeof(T) == typeof(FPSMask) 
                || typeof(T) == typeof(PanicMask) 
                || typeof(T) == typeof(RoseMask))
                return _objects[(int)LAYER_ORDER.MASK].Where(s => s is T).Cast<T>().ToArray();
            
            return new T[0];
        }

        public static bool Render(Graphics g, int x, int y, int w, int h)
        {
            bool update = false;
            bool showlabel = false;
            bool lastIsHouseObj = false;

           // BuildingObject build = null;

            for (int i = 1; i < _objects.Length; i++)
            {
                if (i == (int)LAYER_ORDER.LABELS && !showlabel)
                    continue;

                for (int j = 0; j < _objects[i].Count; j++)
                {
                    RenderObject obj = (RenderObject)_objects[i][j];
                    if (obj != null)
                    {
                        if (obj.IsDisposing)
                        {
                            _objects[i].RemoveAt(j);
                        }
                        else
                        {
                            if (i == (int)LAYER_ORDER.BUILDING || i == (int)LAYER_ORDER.HOUSES)
                            {
                                // horrible patch
                                if (!lastIsHouseObj && obj is BuildingObject buildObj)
                                {
                                    lastIsHouseObj = true;
                                    g.ResetTransform();
                                }

                                bool isover = obj.Render(g, x, y, w, h);
                                showlabel |= isover;

                               /* if (isover)
                                    build = (BuildingObject)obj;*/
                            }                          
                            else
                            {
                                update |= obj.Render(g, x, y, w, h);
                            }
                        }
                    }
                }          
            }

          /*  if (showlabel && build != null)
            {
                Global.CurrentBuildObject = build;
            }
            else if (Global.CurrentBuildObject != null)
                Global.CurrentBuildObject = null;*/

            return update;
        }

        public static bool Render(Graphics g, int w, int h, float angle, short x =-1, short y =-1, int indexToRender = -1)
        {
            bool update = false;

            if (indexToRender >= 0)
            {
                return ((RenderMap)_objects[(int)LAYER_ORDER.BACKGROUND][indexToRender]).Render(g, w, h, angle, x, y);
            }

            for (int i = 0; i < _objects[(int)LAYER_ORDER.BACKGROUND].Count; i++)
            {
                RenderMap map = (RenderMap)_objects[(int)LAYER_ORDER.BACKGROUND][i];
                if (map != null)
                {
                    if (map.IsDisposing)
                    {
                        _objects[(int)LAYER_ORDER.BACKGROUND].RemoveAt(i);
                    }
                    else
                    {
                        update |= map.Render(g, w, h, angle, x, y);
                    }
                }
            }
  

            return update;
        }
    }
}
