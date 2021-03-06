﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using LayoutFarm.RenderBoxes;
using LayoutFarm.CustomWidgets;

namespace LayoutFarm.UI
{
    sealed class GridLayer
    {
        GridTable.GridRowCollection _gridRows;
        GridTable.GridColumnCollection _gridCols;
        int _uniformCellWidth;
        int _uniformCellHeight;
        CellSizeStyle _cellSizeStyle;
        GridTable _gridTable;

        //bool _validSize;
        //int _calculatedWidth;
        //int _calculatedHeight;

#if DEBUG
        static int dbugTotalId;
        public readonly int dbug_layer_id = dbugTotalId++;
#endif

        public GridLayer(int width, int height, CellSizeStyle cellSizeStyle, GridTable gridTable)
        {
            _cellSizeStyle = cellSizeStyle;
            _gridTable = gridTable;
            _gridRows = gridTable.Rows;
            _gridCols = gridTable.Columns;


            int nColumns = gridTable.ColumnCount;
            if (cellSizeStyle == CellSizeStyle.ColumnAndRow)
            {

                int cx = 0;
                for (int c = 0; c < nColumns; c++)
                {
                    GridColumn col = _gridCols.GetColumn(c);
                    int col_w = col.Width;
                    col.Left = cx;
                    cx += col_w;
                }
                //------------------------------------------------------------
                int nRows = gridTable.RowCount;
                if (nRows > 0)
                {

                    int cy = 0;
                    int row_h = 1;
                    for (int r = 0; r < nRows; r++)
                    {
                        GridRow row = _gridRows.GetRow(r);
                        row_h = row.Height;
                        row.Height = row_h;
                        row.Top = cy;
                        cy += row_h;
                    }
                    _uniformCellHeight = row_h;
                }

            }
            else
            {
                int columnWidth = width;
                if (nColumns > 0)
                {
                    columnWidth = columnWidth / nColumns;
                    _uniformCellWidth = columnWidth;
                    if (columnWidth < 1)
                    {
                        columnWidth = 1;
                    }
                }
                //------------------------------------------------------------             
                int cx = 0;
                for (int c = 0; c < nColumns; c++)
                {
                    GridColumn col = _gridCols.GetColumn(c);
                    col.Width = columnWidth;
                    col.Left = cx;
                    cx += columnWidth;
                }
                //------------------------------------------------------------
                int nRows = gridTable.RowCount;
                if (nRows > 0)
                {
                    int rowHeight = height / nRows;
                    int cy = 0;
                    for (int r = 0; r < nRows; r++)
                    {
                        GridRow row = _gridRows.GetRow(r);
                        row.Height = rowHeight;
                        row.Top = cy;
                        cy += rowHeight;
                    }
                    _uniformCellHeight = rowHeight;
                }
            }


            //------------------------------------------------------------
        }
        public bool HitTestCore(HitChain hitChain)
        {
            hitChain.GetTestPoint(out int testX, out int testY);
            GridCell cell = GetCellByPosition(testX, testY);
            if (cell != null && cell.ContentElement is RenderElement renderE)
            {
                hitChain.OffsetTestPoint(-cell.X, -cell.Y);
                renderE.HitTestCore(hitChain);
                hitChain.OffsetTestPoint(cell.X, cell.Y);
                return true;
            }
            return false;
        }

        public int RowCount => _gridRows.Count;

        public void UpdateParentLink(GridViewRenderBox gridViewRenderE)
        {
            int rowCount = _gridRows.Count;
            int colCount = _gridCols.Count;

            for (int c = 0; c < colCount; ++c)
            {
                GridColumn col = _gridCols.GetColumn(c);
                for (int r = 0; r < rowCount; ++r)
                {
                    GridCell gridCell = col.GetCell(r);
                    RenderElement contentRenderE = col.GetCell(r).ContentElement;

#if DEBUG 

                    if (contentRenderE.MyParentLink != null)
                    {
                        throw new NotSupportedException();
                    }

#endif
                    RenderElement.SetParentLink(contentRenderE, gridViewRenderE);
                }
            }
        }


        //        //
        //        public void TopDownReArrangeContent()
        //        {
        //#if DEBUG
        //            //vinv_dbug_EnterLayerReArrangeContent(this);
        //#endif
        //            //--------------------------------- 
        //            //this.BeginLayerLayoutUpdate();
        //            //---------------------------------
        //            if (_gridCols != null && _gridCols.Count > 0)
        //            {
        //                int curY = 0;
        //                foreach (GridRow rowDef in _gridRows.GetRowIter())
        //                {
        //                    rowDef.AcceptDesiredHeight(curY);
        //                    curY += rowDef.Height;
        //                }

        //                int curX = 0;
        //                foreach (GridColumn gridCol in _gridCols.GetColumnIter())
        //                {
        //                    SetLeftAndPerformArrange(gridCol, curX);
        //                    curX += gridCol.Width;
        //                }
        //            }

        //            _validSize = true;

        //            //---------------------------------
        //            //this.EndLayerLayoutUpdate();

        //#if DEBUG
        //            //            vinv_dbug_ExitLayerReArrangeContent();
        //#endif
        //        }


        //static void SetLeftAndPerformArrange(GridColumn col, int left)
        //{
        //    int prevWidth = col.Width;
        //    if (!col.HasCustomSize)
        //    {
        //        col.Width = col.CalculatedWidth;
        //    }
        //    col.Left = left;
        //    int j = col.CellCount;
        //    int dW = col.Width;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        var content = col.GetCell(i).ContentElement as RenderElement;
        //        if (content != null)
        //        {
        //            //RenderElement.DirectSetVisualElementWidth(content, dW);
        //            //if (content.IsVisualContainerBase)
        //            //{

        //            //    ArtVisualContainerBase vscont = (ArtVisualContainerBase)content;
        //            //    vscont.InvalidateContentArrangementFromContainerSizeChanged();
        //            //    vscont.TopDownReArrangeContentIfNeed(vinv);
        //            //} 
        //        }
        //    }
        //}
        public IEnumerable<RenderElement> GetRenderElementIter()
        {
            if (_gridCols != null && _gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in _gridCols.GetColumnIter())
                {
                    foreach (var gridCell in gridCol.GetTopDownGridCellIter())
                    {
                        if (gridCell.ContentElement is RenderElement re)
                        {
                            yield return re;
                        }
                    }
                }
            }
        }
        public IEnumerable<RenderElement> GetRenderElementReverseIter()
        {
            if (_gridCols != null && _gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in _gridCols.GetColumnReverseIter())
                {
                    foreach (var gridCell in gridCol.GetTopDownGridCellIter())
                    {
                        var re = gridCell.ContentElement as RenderElement;
                        if (re != null)
                        {
                            yield return re;
                        }
                    }
                }
            }
        }


        public void ChangeColumnWidth(GridColumn targetGridColumn, int newWidth)
        {
            targetGridColumn.Width = newWidth;
            GridColumn prevColumn = targetGridColumn;
            GridColumn currentColumn = targetGridColumn.NextColumn;
            while (currentColumn != null)
            {
                currentColumn.Left = prevColumn.Right;
                prevColumn = currentColumn;
                currentColumn = currentColumn.NextColumn;
            }
        }
        //
        public int UniformCellWidth => _uniformCellWidth;

        public int UniformCellHeight => _uniformCellHeight;
        //
        public CellSizeStyle GridType => _cellSizeStyle;
        //
        public GridCell GetCellByPosition(int x, int y)
        {
            if (y < 0)
            {
                y = 0;
            }
            if (x < 0)
            {
                x = 0;
            }

            switch (_cellSizeStyle)
            {
                case CellSizeStyle.UniformWidth:
                    {
                        var cell0 = this.GetCell(0, 0);
                        var cellWidth = cell0.Width;
                        GridRow row = _gridRows.GetRowAtPos(y);
                        if (row != null)
                        {
                            int columnNumber = x / cellWidth;
                            if (columnNumber >= _gridCols.Count)
                            {
                                columnNumber = _gridCols.Count - 1;
                            }

                            GridColumn column = _gridCols[columnNumber];
                            if (column == null)
                            {
                                column = _gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    }
                    break;
                case CellSizeStyle.UniformHeight:
                    {
                        var cell0 = this.GetCell(0, 0);
                        var cellHeight = cell0.Height;
                        int rowNumber = y / cellHeight;
                        if (rowNumber >= _gridRows.Count)
                        {
                            rowNumber = _gridRows.Count - 1;
                        }
                        GridRow row = _gridRows[rowNumber];
                        if (row != null)
                        {
                            GridColumn column = _gridCols.GetColumnAtPosition(x);
                            if (column == null)
                            {
                                column = _gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    }
                    break;
                case CellSizeStyle.UniformCell:
                    {
                        //find cell height
                        var cell0 = this.GetCell(0, 0);
                        var cellWidth = cell0.Width;
                        var cellHeight = cell0.Height;
                        int rowNumber = y / cellHeight;
                        if (rowNumber >= _gridRows.Count)
                        {
                            rowNumber = _gridRows.Count - 1;
                        }

                        GridRow row = _gridRows[rowNumber];
                        if (row != null)
                        {
                            int columnNumber = x / cellWidth;
                            if (columnNumber >= _gridCols.Count)
                            {
                                columnNumber = _gridCols.Count - 1;
                            }
                            GridColumn column = _gridCols[columnNumber];
                            if (column == null)
                            {
                                column = _gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    }
                    break;
                default:
                    {
                        GridRow row = _gridRows.GetRowAtPos(y);
                        if (row == null)
                        {
                            row = _gridRows.Last;
                        }
                        if (row != null)
                        {
                            GridColumn column = _gridCols.GetColumnAtPosition(x);
                            if (column == null)
                            {
                                column = _gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    }
                    break;
            }
            return null;
        }
        //
        public GridCell GetCell(int rowIndex, int columnIndex)
        {
            return _gridCols[columnIndex].GetCell(rowIndex);
        }



        public void SetUniformGridItemSize(int cellItemWidth, int cellItemHeight)
        {
            switch (_cellSizeStyle)
            {
                case CellSizeStyle.UniformCell:
                    {
                        _uniformCellWidth = cellItemWidth;
                        _uniformCellHeight = cellItemHeight;
                    }
                    break;
                case CellSizeStyle.UniformHeight:
                    {
                        _uniformCellHeight = cellItemHeight;
                    }
                    break;
                case CellSizeStyle.UniformWidth:
                    {
                        _uniformCellWidth = cellItemWidth;
                    }
                    break;
            }
        }
        internal GridTable.GridRowCollection Rows => _gridRows;
        internal GridTable.GridColumnCollection Columns => _gridCols;

        public void AddNewColumn(int initColumnWidth)
        {
            _gridCols.Add(new GridColumn(initColumnWidth));
        }
        public void AddColumn(GridColumn col)
        {
            _gridCols.Add(col);
        }
        public void InsertColumn(int index, GridColumn col)
        {
            _gridCols.Insert(index, col);
        }
        public void InsertRowAfter(GridRow afterThisRow, GridRow row)
        {
            _gridRows.InsertAfter(afterThisRow, row);
        }
        public GridColumn GetColumnByPosition(int x) => _gridCols.GetColumnAtPosition(x);

        public GridRow GetRowByPosition(int y) => _gridRows.GetRowAtPos(y);

        public void AddRow(GridRow row) => _gridRows.Add(row);

        //
        public int ColumnCount => _gridCols.Count;
        //
        public GridRow GetRow(int index) => _gridRows[index];
        ///
        public GridColumn GetColumn(int index) => _gridCols[index];
        //
        public void AddNewRow(int initRowHeight)
        {
            _gridRows.Add(new GridRow(initRowHeight));
        }
        // 
        public void MoveRowAfter(GridRow fromRow, GridRow toRow)
        {
            _gridRows.MoveRowAfter(fromRow, toRow);

        }
        public void MoveColumnAfter(GridColumn tobeMoveColumn, GridColumn afterColumn)
        {
            _gridCols.MoveColumnAfter(tobeMoveColumn, afterColumn);
        }
        //        public void TopDownReCalculateContentSize()
        //        {
        //            if (_gridRows == null || _gridCols.Count < 1)
        //            {

        //                _calculatedWidth = _calculatedHeight = 0;
        //                return;
        //            }
        //            //---------------------------------------------------------- 
        //            //this.BeginReCalculatingContentSize();
        //            int sumWidth = 0;
        //            int maxHeight = 0;
        //            foreach (GridColumn colDef in _gridCols.GetColumnIter())
        //            {
        //                ReCalculateColumnSize(colDef);
        //                if (!colDef.HasCustomSize)
        //                {
        //                    sumWidth += colDef.DesiredWidth;
        //                }
        //                else
        //                {
        //                    sumWidth += colDef.Width;
        //                }

        //                if (colDef.DesiredHeight > maxHeight)
        //                {
        //                    maxHeight = colDef.DesiredHeight;
        //                }
        //            }
        //            foreach (GridRow rowDef in _gridRows.GetRowIter())
        //            {
        //                rowDef.CalculateRowHeight();
        //            }

        //            if (sumWidth < 1)
        //            {
        //                sumWidth = 1;
        //            }
        //            if (maxHeight < 1)
        //            {
        //                maxHeight = 1;
        //            }

        //            _calculatedWidth = sumWidth;
        //            _calculatedHeight = maxHeight;

        //#if DEBUG
        //            //vinv_dbug_ExitLayerReCalculateContent();
        //#endif


        //        }
        //static void ReCalculateContentSize(GridCell cell)
        //{
        //    if (cell.ContentElement is RenderElement renderE && !renderE.HasCalculatedSize)
        //    {
        //        renderE.TopDownReCalculateContentSize();
        //    }
        //}

        //static void ReCalculateColumnSize(GridColumn col)
        //{
        //    int j = col.CellCount;
        //    if (j > 0)
        //    {
        //        col.DesiredHeight = 0;
        //        bool firstFoundContentCell = false;
        //        int local_desired_width = 0;
        //        for (int i = 0; i < j; i++)
        //        {
        //            GridCell cell = col.GetCell(i);
        //            //ReCalculateContentSize(cell);
        //            int cellDesiredWidth = col.Width;
        //            int cellDesiredHeight = cell.Height;
        //            var content = cell.ContentElement as RenderElement;
        //            if (content != null)
        //            {
        //                if (content.Width > cellDesiredWidth)
        //                {
        //                    cellDesiredWidth = content.Width;
        //                }
        //                if (content.Height > cellDesiredHeight)
        //                {
        //                    cellDesiredHeight = content.Height;
        //                }
        //            }

        //            col.DesiredHeight += cellDesiredHeight;
        //            if (!firstFoundContentCell)
        //            {
        //                firstFoundContentCell = cell.HasContent;
        //            }
        //            if (cellDesiredWidth > local_desired_width)
        //            {
        //                if (firstFoundContentCell)
        //                {
        //                    if (cell.HasContent)
        //                    {
        //                        local_desired_width = cellDesiredWidth;
        //                    }
        //                }
        //                else
        //                {
        //                    local_desired_width = cellDesiredWidth;
        //                }
        //            }
        //        }
        //        col.CalculatedWidth = local_desired_width;
        //    }
        //    else
        //    {
        //        col.CalculatedWidth = col.Width;
        //    }
        //}


#if DEBUG
        public override string ToString()
        {
            return "grid layer (L" + dbug_layer_id + ") postcal:";
        }

#endif

        Color _gridBorderColor = KnownColors.Gray;

        public Color GridBorderColor
        {
            get => _gridBorderColor;
            set => _gridBorderColor = value;
            //invalidate?
        }

        public bool ClipOnEachCell { get; set; }

        public void DrawChildContent(DrawBoard d, UpdateArea updateArea)
        {

            //TODO: temp fixed, review here again,
            if (this.ColumnCount == 0) return;
            //
            GridCell leftTopGridItem = this.GetCell(0, 0);
            if (leftTopGridItem == null)
            {
                return;
            }
            GridCell rightBottomGridItem = this.GetCell(this.RowCount - 1, this.ColumnCount - 1);
            if (rightBottomGridItem == null)
            {
                return;
            }

            GridColumn startColumn = leftTopGridItem.Column;
            GridColumn currentColumn = startColumn;
            GridRow startRow = leftTopGridItem.Row;
            GridColumn stopColumn = rightBottomGridItem.Column.NextColumn;
            GridRow stopRow = rightBottomGridItem.Row.NextRow;
            int startRowId = startRow.RowIndex;
            int stopRowId = 0;
            if (stopRow == null)
            {
                stopRowId = _gridRows.Count;
            }
            else
            {
                stopRowId = stopRow.RowIndex;
            }
            currentColumn = startColumn;
            //----------------------------------------------------------------------------
            Rectangle backup = updateArea.CurrentRect;//backup 
            int enter_canvas_x = d.OriginX;
            int enter_canvas_y = d.OriginY;

            do
            {
                for (int i = startRowId; i < stopRowId; i++)
                {
                    GridCell cell = currentColumn.GetCell(i);
                    if (cell != null && cell.ContentElement is RenderElement renderContent)
                    {

                        //---------------------------
                        //TODO: review here again
                        int x = cell.X;
                        int y = cell.Y;

                        updateArea.CurrentRect = backup;//reset (1)

                        d.SetCanvasOrigin(enter_canvas_x + x, enter_canvas_y + y);
                        updateArea.Offset(-x, -y);

                        if (ClipOnEachCell)
                        {
                            if (d.PushClipAreaRect(cell.Width, cell.Height, updateArea))
                            {
                                RenderElement.Render(renderContent, d, updateArea);
                                d.PopClipAreaRect();
                            }
                        }
                        else
                        {
                            RenderElement.Render(renderContent, d, updateArea);
                        }
                    }
#if DEBUG
                    else
                    {
                        //canvas.DrawText(new char[] { '.' }, gridItem.X, gridItem.Y);
                    }
#endif
                }

                currentColumn = currentColumn.NextColumn;

            } while (currentColumn != stopColumn);

            d.SetCanvasOrigin(enter_canvas_x, enter_canvas_y);


            updateArea.CurrentRect = backup;
            //----------------------
            currentColumn = startColumn;
            int n = 0;

            if (_gridBorderColor.A > 0)
            {

                using (d.SaveStroke())
                {
                    d.StrokeColor = _gridBorderColor;
                    do
                    {
                        GridCell startGridItemInColumn = currentColumn.GetCell(startRowId);
                        GridCell stopGridItemInColumn = currentColumn.GetCell(stopRowId - 1);
                        //draw vertical line
                        d.DrawLine(
                            startGridItemInColumn.Right,
                            startGridItemInColumn.Y,
                            stopGridItemInColumn.Right,
                            stopGridItemInColumn.Bottom);

                        if (n == 0)
                        {
                            //draw horizontal line
                            int horizontalLineWidth = rightBottomGridItem.Right - startGridItemInColumn.X;
                            for (int i = startRowId; i < stopRowId; i++)
                            {
                                GridCell gridItem = currentColumn.GetCell(i);
                                int x = gridItem.X;
                                int gBottom = gridItem.Bottom;
                                d.DrawLine(
                                    x, gBottom,
                                    x + horizontalLineWidth, gBottom);
                            }
                            n = 1;
                        }
                        currentColumn = currentColumn.NextColumn;
                    } while (currentColumn != stopColumn);
                }

            }

            //...

        }

#if  DEBUG
        public void dbug_DumpElementProps(dbugLayoutMsgWriter writer)
        {
            writer.Add(new dbugLayoutMsg(this, this.ToString()));
        }
#endif

    }
}