using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

internal class TreeEnumerator<T> : IAsyncEnumerator<PdfObject>
{
    private PdfArray? currentLeafArray;
    private int currentLeafIndex;
    private readonly Stack<PdfDictionary> pendingIntermediateNodes = new();
        
    public TreeEnumerator(PdfDictionary root)
    {
        pendingIntermediateNodes.Push(root);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public PdfObject Current { get; private set; } = PdfTokenValues.Null;
    private async ValueTask SetCurrentValue() => Current = await currentLeafArray![currentLeafIndex].CA();

    public async ValueTask<bool> MoveNextAsync()
    {
        if (!await FindNextLeafItem().CA()) return false;
        await SetCurrentValue().CA();
        return true;
    }


    private async ValueTask<bool> FindNextLeafItem() => 
        HasNextLeafItem() || await HandleNextIntermediateNode().CA();

    private bool HasNextLeafItem() => currentLeafArray != null && IncrementLeafItem() < currentLeafArray.Count;

    private int IncrementLeafItem() => currentLeafIndex += 2;

    private async ValueTask<bool> HandleNextIntermediateNode()
    {
        while (true)
        {
            if (NoIntermediateNodesLeft()) return false;
            if (await TryFindFirstLeafItem(pendingIntermediateNodes.Pop()).CA()) return true;
        }
    }

    private bool NoIntermediateNodesLeft()
    {
        return pendingIntermediateNodes.Count < 1;
    }

    private async ValueTask<bool> TryFindFirstLeafItem(PdfDictionary node)
    {
        if (await TryPushIntermediateNodeKids(node).CA()) return false;
        return RecordLeafItems(await node.GetAsync<PdfArray>(PdfTreeElementNamer.FinalArrayName<T>()).CA());
    }

    private bool RecordLeafItems(PdfArray leaves)
    {
        if (leaves.Count == 0)
        {
            return false;
        }

        currentLeafArray = leaves;
        currentLeafIndex = 1;
        return true;
    }

    private async Task<bool> TryPushIntermediateNodeKids(PdfDictionary node)
    {
        if (!node.TryGetValue(KnownNames.Kids, out var kidsTask)) return false;
        if (await kidsTask.CA() is not PdfArray kids) return true;
            
        await PushInReverseOrder(kids).CA();

        return true;
    }

    private async Task PushInReverseOrder(PdfArray kids)
    {
        // do not use kids.Reverse because we want to use valuetasks immediately after creating them
        for (var i = kids.Count - 1; i >= 0; i--)
        {
            pendingIntermediateNodes.Push((PdfDictionary)await kids[i].CA());
        }
    }
}