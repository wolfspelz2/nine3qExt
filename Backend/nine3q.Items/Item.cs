using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using nine3q.Items.Aspects;
using nine3q.Lib;

namespace nine3q.Items
{
    public class Item
    {
        #region Basics

        public ItemId Id { get; set; }
        public PropertySet Properties { get; set; }
        public Inventory Inventory { get; set; }

        bool _active = false;

        public Item(Inventory inventory, ItemId id, PropertySet props = null)
        {
            Inventory = inventory;
            Id = id;
            Properties = new PropertySet();

            if (props != null) {
                foreach (var pair in props) {
                    if (pair.Key != Pid.Id) {
                        Set(pair.Key, pair.Value);
                    }
                }
            }

            Properties.Set(Pid.Id, Id);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Id);
            sb.Append(" ");
            if (Properties.ContainsKey(Pid.Name)) {
                sb.Append(GetString(Pid.Name));
                sb.Append(" ");
            }
            sb.Append(Properties.ToString());
            return sb.ToString();
        }

        internal void Activate()
        {
            if (_active) { return; }
            _active = true;

            ForeachAspect(aspect => aspect.OnAspectActivate());
        }

        internal void Deactivate()
        {
            if (!_active) { return; }
            _active = false;

            ForeachAspect(aspect => aspect.OnAspectDeactivate());
        }

        #endregion

        #region Setter

        public void Set(Pid pid, object value)
        {
            var prop = Property.Get(pid);
            value = Property.Normalize(prop.Type, value);

            if (pid == Pid.Name) {
                var name = value as string;
                Inventory.UnsetName(name, Id);
                Inventory.SetName(name, Id);
            }

            var change = new ItemChange() {
                What = Has(pid) ? ItemChange.Variant.SetProperty : ItemChange.Variant.AddProperty,
                ItemId = Id,
                Pid = pid,
                Value = Property.Clone(prop.Type, value),
                PreviousValue = Properties.ContainsKey(pid) ? Properties[pid] : null,
            };

            Properties.Set(pid, value);

            OnPropertyChange(change);
        }

        public bool Delete(Pid pid)
        {
            if (Properties.ContainsKey(pid)) {

                if (pid == Pid.Name) {
                    var name = GetString(Pid.Name);
                    if (!string.IsNullOrEmpty(name)) {
                        Inventory.UnsetName(name, Id);
                    }
                }

                OnPropertyChange(
                new ItemChange() {
                    What = ItemChange.Variant.DeleteProperty,
                    ItemId = Id,
                    Pid = pid,
                    PreviousValue = Properties.ContainsKey(pid) ? Properties[pid] : null,
                });

                Properties.Delete(pid);
                return true;
            }
            return false;
        }

        public void AddToItemSet(Pid nProperty, ItemId nItemId)
        {
            if (Property.Get(nProperty).Type != Property.Type.ItemSet) { throw new Exceptions.WrongItemPropertyTypeException(Inventory.Name, Id, nProperty, Property.Type.ItemSet); }

            OnPropertyChange(
              new ItemChange() {
                  What = ItemChange.Variant.AddItemToCollection,
                  ItemId = Id,
                  Pid = nProperty,
                  ChildId = nItemId
              }
            );

            Properties.AddToItemSet(nProperty, nItemId);
        }

        public void RemoveFromItemSet(Pid nProperty, ItemId nItemId)
        {
            if (Property.Get(nProperty).Type != Property.Type.ItemSet) { throw new Exceptions.WrongItemPropertyTypeException(Inventory.Name, Id, nProperty, Property.Type.ItemSet); }

            OnPropertyChange(
              new ItemChange() {
                  What = ItemChange.Variant.RemoveItemFromCollection,
                  ItemId = Id,
                  Pid = nProperty,
                  ChildId = nItemId
              }
            );

            Properties.RemoveFromItemSet(nProperty, nItemId);
        }

        #endregion

        #region Getter

        public bool Has(Pid pid)
        {
            var value = Properties.Get_maybe_null(pid);
            return value != null;
        }

        // Generic getter
        public void SetInt(Pid pid, long value) { Set(pid, value); }
        public void SetString(Pid pid, string value) { Set(pid, value); }
        public void SetFloat(Pid pid, double value) { Set(pid, value); }
        public void SetBool(Pid pid, bool value) { Set(pid, value); }
        public void SetItem(Pid pid, ItemId value) { Set(pid, value); }
        public void SetItemSet(Pid pid, ItemIdSet value) { Set(pid, value); }

        public object Get(Pid pid)
        {
            var value = Properties.Get_maybe_null(pid);

            // Still null: get from template
            if (value == null) {
                switch (pid) {
                    case Pid.Name:
                    case Pid.TemplateName:
                    case Pid.Id:
                        break;

                    default:
                        var template = GetTemplate();
                        if (template != null) {
                            value = template.Get(pid);
                        }
                        break;
                }
            }

            // Still null: return an appropriate value for the Pid
            if (value == null) {
                switch (pid) {
                    case Pid.Stacksize: value = (long)1; break;
                    case Pid.Slots: value = (long)1000; break;
                }
            }


            // Still null: return an appropriate value for the Type
            if (value == null) {
                value = Property.Default(Property.Get(pid).Type);
            }

            return value;
        }

        public long GetInt(Pid pid) { return (long)Get(pid); }
        public string GetString(Pid pid) { return (string)Get(pid); }
        public double GetFloat(Pid pid) { return (double)Get(pid); }
        public bool GetBool(Pid pid) { return (bool)Get(pid); }
        public ItemId GetItem(Pid pid) { return (ItemId)Get(pid); }
        public ItemIdSet GetItemSet(Pid pid) { return Get(pid) as ItemIdSet; }

        //// Advanced getter

        public T GetEnum<T>(Pid pid, T defaultValue) where T : struct
        {
            var value = (string)Get(pid);
            T result;
            if (!Enum.TryParse(value, out result)) {
                result = defaultValue;
            }
            return result;
        }

        public Item GetTemplate()
        {
            var templateName = GetString(Pid.TemplateName);
            if (!string.IsNullOrEmpty(templateName)) {
                if (Inventory.Templates != null) {
                    if (Inventory.Templates.IsItem(templateName)) {
                        return Inventory.Templates.Item(templateName);
                    }
                }
            }
            return null;
        }

        public JsonPath.Node GetJson(Pid pid)
        {
            try {
                return new JsonPath.Node(GetString(pid));
            } catch (Exception ex) {
                throw new Exceptions.JsonConfigPropertyFormatException(Inventory.Name, Id, pid, ex.Message);
            }
        }

        public PropertySet GetProperties(PidList pids, bool native = false)
        {
            if (pids == PidList.All) {
                return GetProperties_All(native);
            } else if (pids.Count == 1 && pids[0] == Pid.PublicAccess) {
                return GetProperties_Public(native);
            } else if (pids.Count == 1 && pids[0] == Pid.OwnerAccess) {
                return GetProperties_Owner(native);
            }
            return GetProperties_ByPid(pids, native);
        }

        public PropertySet GetProperties_All(bool native = false)
        {
            Item template = native ? null : GetTemplate();

            var props = new PropertySet();
            if (template != null) {
                foreach (var pair in template.GetProperties(PidList.All)) {
                    if (pair.Key != Pid.Name) {
                        props[pair.Key] = pair.Value;
                    }
                }
            }
            foreach (var pair in Properties) {
                props[pair.Key] = pair.Value;
            }
            return props;
        }

        public PropertySet GetProperties_Public(bool native = false)
        {
            return GetProperties_ByAccess(Property.Access.Public, native);
        }

        public PropertySet GetProperties_Owner(bool native = false)
        {
            return GetProperties_ByAccess(Property.Access.Owner, native);
        }

        public PropertySet GetProperties_ByAccess(Property.Access access, bool native = false)
        {
            Item template = native ? null : GetTemplate();

            var props = new PropertySet();
            if (template != null) {
                foreach (var pair in template.GetProperties(PidList.All)) {
                    if (pair.Key != Pid.Name) {
                        if (Property.Get(pair.Key).Access >= access) {
                            props[pair.Key] = pair.Value;
                        }
                    }
                }
            }
            foreach (var pair in Properties) {
                if (Property.Get(pair.Key).Access >= access) {
                    props[pair.Key] = pair.Value;
                }
            }
            return props;
        }

        public PropertySet GetProperties_ByPid(PidList pids, bool native = false)
        {
            Item template = native ? null : GetTemplate();

            var props = new PropertySet();
            foreach (var pid in pids) {
                if (this.Has(pid)) {
                    props[pid] = Properties[pid];
                } else {
                    if (template != null && template.Has(pid) && pid != Pid.Name) {
                        props[pid] = template.Get(pid);
                    }
                }
            }
            return props;
        }

        public IEnumerable<Pid> GetAspects()
        {
            //var pids = new PidList();

            //foreach (var pid in GetProperties(PidList.All).Keys) {
            //    //if (AspectRegistry.Aspects.ContainsKey(pid)) {
            //    if (Property.Get(pid).Group == Property.Group.Aspect) {
            //        pids.Add(pid);
            //    }
            //}

            //return pids;

            var pids1 = GetProperties(PidList.All).Keys;
            var pids2 = AspectRegistry.Aspects.Keys;
            return pids1.Intersect(pids2);
        }

        #endregion

        #region Events

        internal void OnCreate()
        {
            ForeachAspect(aspect => aspect.OnAspectCreate());

            var change = new ItemChange() { What = ItemChange.Variant.CreateItem, ItemId = Id };
            Inventory.OnCreateItem(change);
        }

        internal void OnDelete()
        {
            var containerId = GetItem(Pid.Container);
            if (containerId != ItemId.NoItem) {
                Inventory.Item(containerId).AsContainer().RemoveChild(this);
            }

            ForeachAspect(aspect => aspect.OnAspectDelete());

            var change = new ItemChange() { What = ItemChange.Variant.DeleteItem, ItemId = Id, Item = this };
            Inventory.OnDeleteItem(change);
        }

        internal void OnPropertyChange(ItemChange change)
        {
            Inventory.OnPropertyChange(change);

            ForeachAspect(aspect => aspect.OnAspectPropertyChange(change));
        }

        #endregion

        #region Aspect

        public bool IsAspect(Pid pid)
        {
            return GetBool(pid);
        }

        protected Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                return AspectRegistry.Aspects[pid](this);
            }

            return null;
        }

        public void AssertAspect(Pid pid)
        {
            if (!IsAspect(pid)) {
                throw new Exceptions.WrongItemAspectException(Inventory.Name, Id, pid);
            }
        }

        internal void ForeachAspect(Action<Aspect> action)
        {
            foreach (var key in GetAspects()) {
                var aspect = AsAspect(key);
                if (aspect != null) {
                    action(aspect);
                }
            }
        }

        public int ExecuteAction(string action, PropertySet arguments)
        {
            int countExecuted = 0;

            var passiveId = arguments.GetItem(Pid.Item);

            var actionMap = GetJson(Pid.Actions);
            var mappedActionName = actionMap.Get(action, action);
            if (mappedActionName != action) {
                action = mappedActionName;
            }

            ForeachAspect(aspect => {
                if (aspect.ExecuteAspectAction(action, arguments)) {
                    countExecuted++;
                }
            });

            if (countExecuted == 0) {
                throw new Exceptions.ActionNotAvailableException(Inventory.Name, Id, action);
            }

            return countExecuted;
        }

        #endregion

        #region Timer

        //internal void SetTimer(Pid namePid, Pid intervalPid, string name)
        //{
        //    if (Inventory.IsActive) {
        //        CancelTimer(namePid);
        //        double interval = GetFloat(intervalPid);
        //        if (interval > 0.0) {
        //            var ts = new TimeSpan(0, 0, (int)interval);
        //            Inventory.StartTimer(Id, name, ts);
        //            Set(namePid, name);
        //        }
        //    }
        //}

        //internal void CancelTimer(Pid namePid)
        //{
        //    if (Inventory.IsActive) {
        //        var name = GetString(namePid);
        //        if (!string.IsNullOrEmpty(name)) {
        //            Inventory.CancelTimer(Id, name);
        //        }
        //    }
        //}

        //internal void ActivateTimer(Pid namePid, Pid intervalPid)
        //{
        //    if (Inventory.IsActive) {
        //        var name = GetString(namePid);
        //        if (!string.IsNullOrEmpty(name)) {
        //            double interval = GetFloat(intervalPid);
        //            if (interval > 0.0) {
        //                var ts = new TimeSpan(0, 0, (int)interval);
        //                Inventory.StartTimer(Id, name, ts);
        //            }
        //        }
        //    }
        //}

        internal void OnTimer(string name)
        {
            if (Inventory.IsActive) {
                ForeachAspect(aspect => aspect.OnAspectTimer(name));
            }
        }

        #endregion
    }
}
