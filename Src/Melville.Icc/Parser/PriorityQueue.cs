namespace Melville.Icc.Parser;

    class BBPriorityQueue
    {
        PartialSolution[] heap;
        public int Count { get; private set; }
        public BBPriorityQueue() : this(16) { }
        public BBPriorityQueue(int capacity)
        {
            heap = new PartialSolution[capacity];
        }

        private void TrimEnd(int maxDesired)
        {
            while (Count > 1 && heap[Count - 1].LowerBound >= maxDesired) Count--;
        }
        public void Push(PartialSolution v, int maxDesired)
        {
            TrimEnd(maxDesired);
            if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
            heap[Count] = v;
            SiftUp(Count++);
        }
        public PartialSolution Pop(int maxDesired)
        {
            TrimEnd(maxDesired);
            var v = Top();
            heap[0] = heap[--Count];
            if (Count > 0) SiftDown(0);
            return v;
        }
        public PartialSolution Top()
        {
#if DEBUG
            for (int i = 1; i < Count; i++) 
            {
                if (heap[i].LowerBound < heap[0].LowerBound)
                {
                    throw new InvalidOperationException("Heap is not a heap");
                }
            }
            if (Count > 0) return heap[0];
            throw new InvalidOperationException("No heap");
            
#else
            return heap[0];
#endif
        }
        void SiftUp(int n)
        {
            var v = heap[n];
            for (var n2 = n / 2; 
                n > 0 && v.LowerBound < heap[n2].LowerBound; 
                n = n2, n2 /= 2) heap[n] = heap[n2];
            heap[n] = v;
        }
        void SiftDown(int n)
        {
            var v = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < Count && heap[n2 + 1].LowerBound < heap[n2].LowerBound) n2++;
                if (v.LowerBound <= heap[n2].LowerBound) break;
                heap[n] = heap[n2];
            }
            heap[n] = v;
        }
    }
