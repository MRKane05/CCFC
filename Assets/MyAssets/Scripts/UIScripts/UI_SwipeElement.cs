using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum DraggedDirection
{
	Up,
	Down,
	Right,
	Left
}

public class UI_SwipeElement : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public void OnEndDrag(PointerEventData eventData)
	{
		Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
		DraggedDirection draggedDir = GetDragDirection(dragVectorDirection);
		switch (draggedDir)
        {
			case DraggedDirection.Left:
				((LevelController)LevelControllerBase.Instance).requestNextTarget(PlayerController.Instance, PlayerController.Instance.targetController, -1);
				break;
			case DraggedDirection.Right:
				((LevelController)LevelControllerBase.Instance).requestNextTarget(PlayerController.Instance, PlayerController.Instance.targetController, 1);
				break;
		}

	}

	private DraggedDirection GetDragDirection(Vector3 dragVector)
	{
		float positiveX = Mathf.Abs(dragVector.x);
		float positiveY = Mathf.Abs(dragVector.y);
		DraggedDirection draggedDir;
		if (positiveX > positiveY)
		{
			draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
		}
		else
		{
			draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
		}
		Debug.Log(draggedDir);
		return draggedDir;
	}

    public void OnDrag(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }
}
