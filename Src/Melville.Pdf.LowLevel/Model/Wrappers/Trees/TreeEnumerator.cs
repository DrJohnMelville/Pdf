using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

internal class TreeEnumerator: IAsyncEnumerator<PdfDirectObject>
{
    private PdfArray? currentLeafArray;
    private int currentLeafIndex;
    private readonly Stack<PdfDictionary> pendingIntermediateNodes = new();
        
    public TreeEnumerator(PdfDictionary root)
    {
        pendingIntermediateNodes.Push(root);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public PdfDirectObject Current { get; private set; } = default;
    private async ValueTask SetCurrentValueAsync() => Current = await currentLeafArray![currentLeafIndex].CA();

    public async ValueTask<bool> MoveNextAsync()
    {
        if (!await FindNextLeafItemAsync().CA()) return false;
        await SetCurrentValueAsync().CA();
        return true;
    }


    private async ValueTask<bool> FindNextLeafItemAsync() => 
        HasNextLeafItem() || await HandleNextIntermediateNodeAsync().CA();

    private bool HasNextLeafItem() => currentLeafArray != null && IncrementLeafItem() < currentLeafArray.Count;

    private int IncrementLeafItem() => currentLeafIndex += 2;

    private async ValueTask<bool> HandleNextIntermediateNodeAsync()
    {
        while (true)
        {
            if (NoIntermediateNodesLeft()) return false;
            if (await TryFindFirstLeafItemAsync(pendingIntermediateNodes.Pop()).CA()) return true;
        }
    }

    private bool NoIntermediateNodesLeft()
    {
        return pendingIntermediateNodes.Count < 1;
    }

    private async ValueTask<bool> TryFindFirstLeafItemAsync(PdfDictionary node)
    {
        if (await TryPushIntermediateNodeKidsAsync(node).CA()) return false;
        return RecordLeafItems(
            (await node.GetWithAlternativeNameAsync(
                KnownNames.Nums, KnownNames.Names).CA()).Get<PdfArray>());
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

    private async Task<bool> TryPushIntermediateNodeKidsAsync(PdfDictionary node)
    {
        if (!node.TryGetValue(KnownNames.Kids, out var kidsTask)) return false;
        if (!(await kidsTask.CA()).TryGet(out PdfArray? kids)) return true;
            
        await PushInReverseOrderAsync(kids).CA();

        return true;
    }

    private async Task PushInReverseOrderAsync(PdfArray kids)
    {
        // do not use kids.Reverse because we want to use valuetasks immediately after creating them
        for (var i = kids.Count - 1; i >= 0; i--)
        {
            pendingIntermediateNodes.Push((await kids[i].CA()).Get<PdfDictionary>());
        }
    }
}