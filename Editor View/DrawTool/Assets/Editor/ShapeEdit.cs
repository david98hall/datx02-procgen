﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sebastian.Geometry;

[CustomEditor(typeof(ShapeMake))]
public class ShapeEdit : Editor {

    ShapeMake shapeMake;
    SelectionInfo selectionInfo;
    bool needsRepaint;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int shapeDeleteIndex = -1;
        shapeMake.showShapesList = EditorGUILayout.Foldout(shapeMake.showShapesList, "Shapes");
        if(shapeMake.showShapesList)
        {
            for (int i = 0; i < shapeMake.shapes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Shape " + (i + 1));
                GUI.enabled = i != selectionInfo.selectedShapeIndex;
                if(GUILayout.Button("Select"))
                {
                    selectionInfo.selectedShapeIndex = i;
                }
                GUI.enabled = true;
                if (GUILayout.Button("Delete"))
                {
                     shapeDeleteIndex = i;
                }
                GUILayout.EndHorizontal();
            }
        }
        
        if(shapeDeleteIndex != -1)
        {
            Undo.RecordObject(shapeMake,"Delete shape");
            shapeMake.shapes.RemoveAt(shapeDeleteIndex);
            selectionInfo.selectedShapeIndex = Mathf.Clamp(selectionInfo.selectedShapeIndex, 0, shapeMake.shapes.Count-1);
        }
        if(GUI.changed)
        {
            needsRepaint = true;
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        Event guiEvent = Event.current;

        if (guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
			if (needsRepaint)
			{
				HandleUtility.Repaint();
			}
        }
    }

    void CreateNewShape()
    {
        Undo.RecordObject(shapeMake, "Create shape");
        shapeMake.shapes.Add(new Shape());
        selectionInfo.selectedShapeIndex = shapeMake.shapes.Count - 1;
    }

    void CreateNewPoint(Vector3 position)
    {
        bool mouseIsOverSelectedShape = selectionInfo.mouseOverShapeIndex == selectionInfo.selectedShapeIndex;
        int newPointIndex = (selectionInfo.mouseIsOverLine && mouseIsOverSelectedShape) ? selectionInfo.lineIndex + 1 : SelectedShape.points.Count;
		Undo.RecordObject(shapeMake, "Add point");
        SelectedShape.points.Insert(newPointIndex, position);
		selectionInfo.pointIndex = newPointIndex;
        selectionInfo.mouseOverShapeIndex = selectionInfo.selectedShapeIndex;
        needsRepaint = true;

        SelectPointUnderMouse();
    }

    void SelectPointUnderMouse()
    {
        selectionInfo.pointIsSelected = true;
        selectionInfo.mouseIsOverPoint = true;
        selectionInfo.mouseIsOverLine = false;
        selectionInfo.lineIndex = -1;

        selectionInfo.positionAtStartOfDrag = SelectedShape.points[selectionInfo.pointIndex];
        needsRepaint = true;
    }

    void SelectShapeUnderMouse()
    {
        if (selectionInfo.mouseOverShapeIndex != -1)
        {
            selectionInfo.selectedShapeIndex = selectionInfo.mouseOverShapeIndex;
            needsRepaint = true;
        }
    }

    void HandleInput(Event guiEvent)
    {
		Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
		float drawPlaneHeight = 0;
		float dstToDrawPlane = (drawPlaneHeight - mouseRay.origin.y) / mouseRay.direction.y;
		Vector3 mousePosition = mouseRay.GetPoint(dstToDrawPlane);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
		{
            HandleShiftLeftMouseDown(mousePosition);
		}

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
		{
            HandleLeftMouseDown(mousePosition);
		}

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
        {
            HandleLeftMouseUp(mousePosition);
        }

        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleLeftMouseDrag(mousePosition);
        }
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.modifiers == EventModifiers.None)
        {
            HandleRightMouseDown(mousePosition);
        }

        if (!selectionInfo.pointIsSelected)
        {
            UpdateMouseOverInfo(mousePosition);
        }

	}

    void HandleShiftLeftMouseDown(Vector3 mousePosition)
    {
        CreateNewShape();
        CreateNewPoint(mousePosition);
    }

    void HandleLeftMouseDown(Vector3 mousePosition)
    {
        if (shapeMake.shapes.Count == 0)
        {
            CreateNewShape();
        }

        SelectShapeUnderMouse();

        if (selectionInfo.mouseIsOverPoint)
        {
            SelectPointUnderMouse();
        }
        else
        {
            CreateNewPoint(mousePosition);
        }
    }

	void HandleLeftMouseUp(Vector3 mousePosition)
	{
        if (selectionInfo.pointIsSelected)
        {
            SelectedShape.points[selectionInfo.pointIndex] = selectionInfo.positionAtStartOfDrag;
            Undo.RecordObject(shapeMake, "Move point");
            SelectedShape.points[selectionInfo.pointIndex] = mousePosition;

            selectionInfo.pointIsSelected = false;
            selectionInfo.pointIndex = -1;
            needsRepaint = true;
        }

	}

	void HandleLeftMouseDrag(Vector3 mousePosition)
	{
        if (selectionInfo.pointIsSelected)
        {
            SelectedShape.points[selectionInfo.pointIndex] = mousePosition;
            needsRepaint = true;
        }

	}
    void HandleRightMouseDown(Vector3 mousepos)
	{
		if(selectionInfo.mouseIsOverPoint)
		{
            SelectShapeUnderMouse();
			Undo.RecordObject(shapeMake, "Remove point");
			SelectedShape.points.RemoveAt(selectionInfo.pointIndex);
            selectionInfo.pointIsSelected = false;
            selectionInfo.mouseIsOverPoint = false;
			needsRepaint = true;
		}
	}

    void UpdateMouseOverInfo(Vector3 mousePosition)
    {
        int mouseOverPointIndex = -1;
        int mouseOverShapeIndex = -1;
        for (int shapeIndex = 0; shapeIndex < shapeMake.shapes.Count; shapeIndex++)
        {
            Shape currentShape = shapeMake.shapes[shapeIndex];

            for (int i = 0; i < currentShape.points.Count; i++)
            {
                if (Vector3.Distance(mousePosition, currentShape.points[i]) < shapeMake.handleRadius)
                {
                    mouseOverPointIndex = i;
                    mouseOverShapeIndex = shapeIndex;
                    break;
                }
            }
        }

        if (mouseOverPointIndex != selectionInfo.pointIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
        {
            selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
            selectionInfo.pointIndex = mouseOverPointIndex;
            selectionInfo.mouseIsOverPoint = mouseOverPointIndex != -1;

            needsRepaint = true;
        }

        if (selectionInfo.mouseIsOverPoint)
        {
            selectionInfo.mouseIsOverLine = false;
            selectionInfo.lineIndex = -1;
        }
        else
        {
            int mouseOverLineIndex = -1;
            float closestLineDst = shapeMake.handleRadius;
            for (int shapeIndex = 0; shapeIndex < shapeMake.shapes.Count; shapeIndex++)
            {
                Shape currentShape = shapeMake.shapes[shapeIndex];

                for (int i = 0; i < currentShape.points.Count; i++)
                {
                    Vector3 nextPointInShape = currentShape.points[(i + 1) % currentShape.points.Count];
                    float dstFromMouseToLine = HandleUtility.DistancePointToLineSegment(mousePosition.ToXZ(), currentShape.points[i].ToXZ(), nextPointInShape.ToXZ());
                    if (dstFromMouseToLine < closestLineDst)
                    {
                        closestLineDst = dstFromMouseToLine;
                        mouseOverLineIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                    }
                }
            }

            if (selectionInfo.lineIndex != mouseOverLineIndex || mouseOverShapeIndex != selectionInfo.mouseOverShapeIndex)
            {
                selectionInfo.mouseOverShapeIndex = mouseOverShapeIndex;
                selectionInfo.lineIndex = mouseOverLineIndex;
                selectionInfo.mouseIsOverLine = mouseOverLineIndex != -1;
                needsRepaint = true;
            }
        }
    }

    void Draw()
    {
        for (int shapeIndex = 0; shapeIndex < shapeMake.shapes.Count; shapeIndex++)
        {
            Shape shapeToDraw = shapeMake.shapes[shapeIndex];
            bool shapeIsSelected = shapeIndex == selectionInfo.selectedShapeIndex;
            bool mouseIsOverShape = shapeIndex == selectionInfo.mouseOverShapeIndex;
            Color deselectedShapeColour = Color.grey;

            for (int i = 0; i < shapeToDraw.points.Count; i++)
            {
                Vector3 nextPoint = shapeToDraw.points[(i + 1) % shapeToDraw.points.Count];
                if (i == selectionInfo.lineIndex && mouseIsOverShape)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawLine(shapeToDraw.points[i], nextPoint);
                }
                else
                {
                    Handles.color = (shapeIsSelected)?Color.white:deselectedShapeColour;
                    Handles.DrawDottedLine(shapeToDraw.points[i], nextPoint, 4);
                }

                if (i == selectionInfo.pointIndex && mouseIsOverShape)
                {
                    Handles.color = (selectionInfo.pointIsSelected) ? Color.green : Color.cyan;
                }
                else
                {
                    Handles.color = (shapeIsSelected)?Color.white:deselectedShapeColour;
                }
                Handles.DrawSolidDisc(shapeToDraw.points[i], Vector3.up, shapeMake.handleRadius);
            }
        }
        if (needsRepaint)
        {
            shapeMake.UpdateMesh();
        }
        needsRepaint = false;
    }

    void OnEnable()
    {
        needsRepaint = true;
        shapeMake = target as ShapeMake;
        selectionInfo = new SelectionInfo();
        Undo.undoRedoPerformed += OnUndoOrRedo;
        Tools.hidden = true;
    }

    void OnDisable()
    {
		Undo.undoRedoPerformed -= OnUndoOrRedo;
        Tools.hidden = false;
    }

    void OnUndoOrRedo()
    {
        if (selectionInfo.selectedShapeIndex >= shapeMake.shapes.Count || selectionInfo.selectedShapeIndex == -1)
        {
            selectionInfo.selectedShapeIndex = shapeMake.shapes.Count - 1;
        }
    }

    Shape SelectedShape
    {
        get
        {
            return shapeMake.shapes[selectionInfo.selectedShapeIndex];
        }
    }

    public class SelectionInfo
    {
        public int selectedShapeIndex;
        public int mouseOverShapeIndex;

        public int pointIndex = -1;
        public bool mouseIsOverPoint;
        public bool pointIsSelected;
        public Vector3 positionAtStartOfDrag;

        public int lineIndex = -1;
        public bool mouseIsOverLine;
    }

}
