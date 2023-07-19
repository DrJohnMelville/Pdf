using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Trees;

internal class TreeEnumerator: IAsyncEnumerator<PdfDirectValue>
{
    private PdfValueArray? currentLeafArray;
    private int currentLeafIndex;
    private readonly Stack<PdfValueDictionary> pendingIntermediateNodes = new();
        
    public TreeEnumerator(PdfValueDictionary root)
    {
        pendingIntermediateNodes.Push(root);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public PdfDirectValue Current { get; private set; } = default;
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

    private async ValueTask<bool> TryFindFirstLeafItemAsync(PdfValueDictionary node)
    {
        if (await TryPushIntermediateNodeKidsAsync(node).CA()) return false;
        return RecordLeafItems(
            (await node.GetWithAlternativeName(
                KnownNames.NumsTName, KnownNames.NamesTName).CA()).Get<PdfValueArray>());
    }

    private bool RecordLeafItems(PdfValueArray leaves)
    {
        if (leaves.Count == 0)
        {
            return false;
        }

        currentLeafArray = leaves;
        currentLeafIndex = 1;
        return true;
    }

    private async Task<bool> TryPushIntermediateNodeKidsAsync(PdfValueDictionary node)
    {
        if (!node.TryGetValue(KnownNames.KidsTName, out var kidsTask)) return false;
        if (!(await kidsTask.CA()).TryGet(out PdfValueArray kids)) return true;
            
        await PushInReverseOrderAsync(kids).CA();

        return true;
    }

    private async Task PushInReverseOrderAsync(PdfValueArray kids)
    {
        // do not use kids.Reverse because we want to use valuetasks immediately after creating them
        for (var i = kids.Count - 1; i >= 0; i--)
        {
            pendingIntermediateNodes.Push((await kids[i].CA()).Get<PdfValueDictionary>());
        }
    }
}