using Haus.HassClient;

namespace Haus.Commands.Label;

// Labels are stored as a full array on each entity/area registry entry, so assign/remove is
// read-modify-write: pull the current labels, add or drop one, write the set back.
internal static class LabelAssignment
{
    public static async Task<(int changed, int total)> ApplyAsync(
        IHassClient client, string labelId, IReadOnlyList<string> entities, IReadOnlyList<string> areas,
        bool add, CancellationToken cancellationToken)
    {
        var changed = 0;
        var total = 0;

        foreach (var entityId in entities)
        {
            total++;
            var entry = await client.EntityRegistry.GetAsync(entityId, cancellationToken)
                ?? throw new InvalidOperationException($"Entity not found in registry: {entityId}");
            var labels = new HashSet<string>(entry.Labels ?? []);
            if (add ? labels.Add(labelId) : labels.Remove(labelId))
            {
                await client.EntityRegistry.UpdateAsync(entityId, new(Labels: labels.ToList()), cancellationToken);
                changed++;
            }
        }

        foreach (var areaId in areas)
        {
            total++;
            var entry = await client.Area.GetAsync(areaId, cancellationToken)
                ?? throw new InvalidOperationException($"Area not found in registry: {areaId}");
            var labels = new HashSet<string>(entry.Labels ?? []);
            if (add ? labels.Add(labelId) : labels.Remove(labelId))
            {
                await client.Area.UpdateAsync(areaId, new(Labels: labels.ToList()), cancellationToken);
                changed++;
            }
        }

        return (changed, total);
    }
}
