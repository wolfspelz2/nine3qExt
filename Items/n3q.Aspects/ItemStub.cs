using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using n3q.Common;
using n3q.GrainInterfaces;
using n3q.Items;
using n3q.Tools;

namespace n3q.Aspects
{
    public class ItemStub
    {
        public IItemClient Client;
        public ITransaction Transaction;

        public string Id => Client.GetId();

        public ItemStub(IItemClient itemClient, ITransaction transaction = null)
        {
            Client = itemClient;
            Transaction = transaction;
        }

        public IItem Grain => Client.GetItem();

        public async Task<ItemReader> ReadonlyItem(string itemId)
        {
            if (!Has.Value(itemId)) {
                throw new Exception($"{nameof(Aspect)}.{nameof(ReadonlyItem)}: Empty or null itemId");
            }

            var client = Client.CloneFor(itemId);
            var item = new ItemReader(client, Transaction);
            await Task.CompletedTask; //await Transaction?.AddItem(item);
            return item;
        }

        public async Task<ItemWriter> WritableItem(string itemId)
        {
            if (!Has.Value(itemId)) {
                throw new Exception($"{nameof(Aspect)}.{nameof(WritableItem)}: Empty or null itemId");
            }

            var client = Client.CloneFor(itemId);
            var item = new ItemWriter(client, Transaction);
            await Transaction?.AddItem(item);
            return item;
        }

        public async Task<ItemWriter> NewItemFromTemplate(string tmpl)
        {
            var shortTmpl = tmpl.Substring(0, Cluster.LengthOfItemIdPrefixFromTemplate);
            var itemId = $"{shortTmpl}{RandomString.GetAlphanumLowercase(20)}";
            itemId = itemId.ToLower();
            var item = await WritableItem(itemId);

            await item.Modify(new PropertySet { [Pid.Template] = tmpl }, PidSet.Empty);
            
            return item;
        }

        #region Aspects

        public delegate Task ActionAsync<in T>(T obj);

        public async Task ForeachAspect(ActionAsync<Aspect> action)
        {
            foreach (var key in await GetAspects()) {
                var aspect = AsAspect(key);
                if (aspect != null) {
                    await action(aspect);
                }
            }
        }

        public async Task<IEnumerable<Pid>> GetAspects()
        {
            var aspectProps = await Get(PidSet.Aspects);
            var itemAspectPids = aspectProps.Keys;
            var knownAspectPids = AspectRegistry.Aspects.Keys;
            return itemAspectPids.Intersect(knownAspectPids);
        }

        public Aspect AsAspect(Pid pid)
        {
            if (AspectRegistry.Aspects.ContainsKey(pid)) {
                var aspect = AspectRegistry.Aspects[pid](this);
                return aspect;
            }
            throw new Exception($"{nameof(ItemStub)}.{nameof(AsAspect)}: Unknown pid/aspect={pid}");
        }

        #endregion

        #region IItem stubs

        public async Task<PropertySet> Get(PidSet pids, bool native = false)
        {
            var t = Transaction;
            var result = await Grain.GetProperties(pids, native);
            if (Transaction == null) {
                Transaction = t;
            }
            return result;
        }

        public async Task BeginTransaction() { AssertTransaction(); await Grain.BeginTransaction(Transaction.Id); }
        public async Task EndTransaction(bool success) { AssertTransaction(); await Grain.EndTransaction(Transaction.Id, success); }

        public async Task<PropertyValue> Get(Pid pid)
        {
            var props = await Get(new PidSet { pid });
            if (props.TryGetValue(pid, out var value)) {
                return value;
            }
            return PropertyValue.Empty;
        }

        public async Task<string> GetString(Pid pid) { return await Get(pid); }
        public async Task<long> GetInt(Pid pid) { return await Get(pid); }
        public async Task<double> GetFloat(Pid pid) { return await Get(pid); }
        public async Task<bool> GetBool(Pid pid) { return await Get(pid); }
        public async Task<string> GetItemId(Pid pid) { return await Get(pid); }
        public async Task<ValueList> GetItemIdList(Pid pid) { return await Get(pid); }
        public async Task<ValueList> GetList(Pid pid) { return await Get(pid); }
        public async Task<ValueMap> GetMap(Pid pid) { return await Get(pid); }

        public async Task Deactivate() { await Grain.Deactivate(); }
        public async Task DeletePersistentStorage() { await Grain.DeletePersistentStorage(); }

        public delegate Task TransactionWrappedCode(ItemWriter item);

        public async Task WithTransactionCore(TransactionWrappedCode transactedCode, ITransaction transaction)
        {
            Transaction = transaction;
            await Transaction.Begin(this as ItemWriter);
            try {
                await transactedCode(this as ItemWriter);
                await Transaction.Commit();
            } catch (Exception ex) {
                _ = ex;
                await Transaction.Cancel();
                throw;
            } finally {
                Transaction = null;
            }
        }

        public async Task WithTransaction(TransactionWrappedCode transactedCode)
        {
            await WithTransactionCore(transactedCode, new ItemTransaction());
        }

        public async Task WithoutTransaction(TransactionWrappedCode transactedCode)
        {
            await WithTransactionCore(transactedCode, new VoidTransaction());
        }

        public async Task<Dictionary<Pid, string>> Execute(string actionName, Dictionary<string, string> args)
        {
            var executedActions = new Dictionary<Pid, string>();

            var actionMap = await GetMap(Pid.Actions);
            if (!actionMap.TryGetValue(actionName, out var mappedActionName)) {
                mappedActionName = actionName;
            }

            await ForeachAspect(async aspect => {
                var actions = aspect.GetActionList();
                if (actions != null) {
                    if (actions.ContainsKey(mappedActionName)) {

                        PropertySet mappedArguments = Aspect.MapArgumentsToAspectAction(args, aspect, mappedActionName);
                        await actions[mappedActionName].Handler(mappedArguments);
                        executedActions.Add(aspect.GetAspectPid(), mappedActionName);

                    }
                }
            });

            return executedActions;
        }

        protected  void AssertTransaction()
        {
            if (Transaction == null) {
                throw new Exception("No transaction");
            }
        }

        protected  void AssertStubMethodIsUsed()
        {
            throw new Exception($"Do not use the interface directly. Please use the stub method {nameof(Get)}");
        }

        #endregion
    }
}
