using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    public abstract class HashDictionary<T> where T : class
    {
        // the dictionary either holds the value, a synonym declaration, or a linkedListPair
        // The idea of this class is that FNV-1 is a very good hash, so there will be very, very
        // few colisions.  Because of this when there is only one item in the bucket then we
        // have no allocations and pay nothing other than a single is operation.  If there is more
        // than one item in a bucket we degrade to a linked list.  This also lets us handle
        // synonyms.  We give up type safety to do this.
        private readonly Dictionary<uint, object> store = new();

        public void ForceAdd(in ReadOnlySpan<byte> key, T item)
        {
            uint hash = FnvHash.FnvHashAsUint(key);
            store.Add(hash, item);
        }
        
        public T GetOrCreate(in ReadOnlySpan<byte> key) => GetOrCreate(key, null);
        internal T GetOrCreate(in ReadOnlySpan<byte> key, byte[]? keyAsArray)
        {
            var hash = FnvHash.FnvHashAsUint(key);
            if (!store.TryGetValue(hash, out var found))
            {
                var ret = Create(key, keyAsArray);
                store.Add(hash, ret);
                return ret;
            }
            if (SearchLinkedList(found, key, out var foundRet)) return foundRet;
            var newHead = new SingleLinkedList(Create(key, keyAsArray), found);
            store[hash] = newHead;
            return newHead.Datum;
        }
        
        private bool SearchLinkedList(
            object source, in ReadOnlySpan<byte> key, [NotNullWhen(true)] out T? ret) =>
            source switch
            {
                T item => DoComparison(key, item, out ret),
                Synonym syn => syn.CheckKey(key, out ret),
                SingleLinkedList sll =>
                    DoComparison(key, sll.Datum, out ret) ||
                    SearchLinkedList(sll.Next, key, out ret),
                _=> throw new InvalidProgramException("source should be one of the prior 3 cases")
            };

        private bool DoComparison(
            ReadOnlySpan<byte> key, T item, [NotNullWhen(true)] out T? target)
        {
            target = item;
            return Matches(key, item);
        }
        
        private class SingleLinkedList
        {
            public readonly T Datum;
            public readonly object Next;

            public SingleLinkedList(T datum, object next)
            {
                Datum = datum;
                Next = next;
            }
        }

        private class Synonym
        {
            private readonly byte[] Key;
            private readonly T Target;

            public Synonym(byte[] key, T target)
            {
                Key = key;
                Target = target;
            }

            public bool CheckKey(in ReadOnlySpan<byte> checkKey, out T item)
            {
                item = Target;
                return checkKey.SequenceEqual(Key);
            }
        }
        
        public void AddSynonym(byte[] key, T target)
        {
            var hash = FnvHash.FnvHashAsUint(key);
            // we set the synonyms first and there is no colision between the synonyms.
            // this means that a synonym will always be the end of its linked list and
            // we get to skip checking SingleLinkedList nodes for synonyms
            store.Add(hash, new Synonym(key, target));
        }

        protected abstract bool Matches(in ReadOnlySpan<byte> key, T item);
        protected abstract T Create(byte[] key);
        
        private T Create(ReadOnlySpan<byte> key, byte[]? keyAsArray) => 
            Create(keyAsArray ?? key.ToArray());
    }

    public class NameDictionay : HashDictionary<PdfName>
    {
        protected override PdfName Create(byte[] key) => new(key);
        protected override bool Matches(in ReadOnlySpan<byte> key, PdfName item) => 
            key.SequenceEqual(item.Bytes);
    }
}