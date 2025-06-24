using System.Numerics;

namespace Game
{
    class Entity
    {
        public Vector2 Position;
        public Vector2 VisualOffset = Vector2.Zero;
        public Queue<IAction> Actions = new();
        public Voxel Voxel;

        public Entity(Vector2 position, Voxel voxel)
        {
            Position = position;
            Voxel = voxel;
        }

        public void Enqueue(IAction action) => Actions.Enqueue(action);
    }

    class Simulation
    {
	private List<Entity> _entities = new();
	private Ref<IMap> MapRef { get; }

	public Simulation(Ref<IMap> mapRef)
	{
	    MapRef = mapRef;
	}

	public void AddEntity(Entity entity, int x, int y, int z)
	{
	    _entities.Add(entity);
	    MapRef.Value.Set(x, y, z, entity.Voxel);
	}

	public void Tick()
	{
	    foreach (var entity in _entities)
	    {
		if (entity.Actions.Count == 0) continue;

		var action = entity.Actions.Dequeue();
		switch (action)
		{
		    case MoveAction move:
			HandleMove(entity, move.Target);
			break;

		    case WaitAction wait:
			// Just skip the tick or implement duration later
			break;

		    case SayAction say:
			System.Console.WriteLine($"Entity at {entity.Position} says: {say.Message}");
			break;
		}
	    }
	}

	private void HandleMove(Entity entity, Vector2 target)
	{
	    if (!IsPassable(target) || IsOccupied(target)) return;

	    MapRef.Value.Set((int)entity.Position.X, (int)entity.Position.Y, 0, null);
	    entity.Position = target;
	    MapRef.Value.Set((int)target.X, (int)target.Y, 0, entity.Voxel);
	}

	private bool IsPassable(Vector2 pos)
	{
	    var voxel = MapRef.Value.Get((int)pos.X, (int)pos.Y, 0);
	    return voxel != null && voxel.Type != "water";
	}

	private bool IsOccupied(Vector2 pos)
	{
	    // TODO: Implement real check later
	    return false;
	}
    }
}
