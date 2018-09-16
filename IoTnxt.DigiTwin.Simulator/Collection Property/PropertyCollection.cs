using System.Collections.Generic;
using System.Linq;
using IoTnxt.Common.Extensions;

namespace IoTnxt.DigiTwin.Simulator.Collection_Property
{
    /// <summary>
    ///  Generic collection to be used to deserialize and manage property collections
    ///  To use: inherit this class and provide tracked type i.e. <T>
    ///  See RFIDCollection class for reference
    /// </summary>
    public class PropertyCollection<T> : Dictionary<string, Dictionary<string, T>>
    {
        public static Dictionary<string, Dictionary<string, T>> CollectionTracking = new Dictionary<string, Dictionary<string, T>>();

        public static Dictionary<string, T> AddUpdateCollection(string collectionIdentifier, Dictionary<string, T> changeCollection)
        {
            if (!CollectionTracking.TryGetValue(collectionIdentifier, out var previousValues))
                CollectionTracking[collectionIdentifier] = previousValues = new Dictionary<string, T>();

            var addedUpdatedCollection = new Dictionary<string, T>();

            foreach (var collectionItem in changeCollection)
            {
                if (!previousValues.Keys.Contains(collectionItem.Key))
                {
                    addedUpdatedCollection.Add(collectionItem.Key, collectionItem.Value);
                    continue;
                }

                if (!collectionItem.Value.Equals(previousValues[collectionItem.Key]))
                    addedUpdatedCollection[collectionItem.Key] = collectionItem.Value;
            }

            return addedUpdatedCollection;
        }

        public static Dictionary<string, T> RemoveFromCollection(string collectionIdentifier, Dictionary<string, T> changeCollection)
        {
            var removeCollection = new Dictionary<string, T>();

            if (CollectionTracking.ContainsKey(collectionIdentifier))
            {
                var collection = CollectionTracking[collectionIdentifier];

                removeCollection = collection.Where(x => !changeCollection.Keys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            }               

            return removeCollection;
        }

        public static Dictionary<string, T> ClearCollection(string collectionIdentifier)
        {
            var removeCollection = new Dictionary<string, T>();

            if (!CollectionTracking.ContainsKey(collectionIdentifier)) 
                return removeCollection;

            var collection = CollectionTracking[collectionIdentifier];
                
            foreach(var collectionItem in collection)
                removeCollection.Add(collectionItem.Key, collectionItem.Value);

            collection.Clear();

            return removeCollection;
        }

        public static void UpdateCollectionTracking(string collectionIdentifier,Dictionary<string, T> addUpdateCollection, Dictionary<string, T> removeCollection)
        {
            if (!CollectionTracking.ContainsKey(collectionIdentifier)) 
                return;

            var collection = CollectionTracking[collectionIdentifier];

            foreach(var addedUpdatedItem in addUpdateCollection)
            {
                if (!collection.Keys.Contains(addedUpdatedItem.Key))
                {
                    collection.Add(addedUpdatedItem.Key, addedUpdatedItem.Value);
                    continue;
                }

                if (addedUpdatedItem.Value.Equals(collection[addedUpdatedItem.Key])) 
                    continue;

                collection[addedUpdatedItem.Key] = addedUpdatedItem.Value;
            }

            collection.RemoveAll(removeCollection.ContainsKey);
        }
    }    
}