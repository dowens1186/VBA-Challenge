Sub Main()
	Dim wSheet As Worksheet
	For Each ws In Worksheets
		If InStr(1, ws.Name, "Res_") = 1 Then
            Exit For
        End If
		Set wSheet = ws
		' Set ws = FindOrCreateSheet("Res_ " + wSheet.name)
		Call FormatwsSheet(wSheet)
		Call RecordDataOnSheet(wSheet)
		Call BonusContent(wSheet)
		wSheet.Range("A:G").Columns.AutoFit
		wSheet.Range("J:O").Columns.AutoFit
		wSheet.Range("R:T").Columns.AutoFit
		wSheet.Range("H:I").Columns.ColumnWidth = 3
		wSheet.Range("P:Q").Columns.ColumnWidth = 3
		wSheet.Range("K1").EntireColumn.Hidden = True
		wSheet.Range("L1").EntireColumn.Hidden = True
    Next ws
End Sub
Function RecordDataOnSheet(ws As Worksheet)
	' Create all the nessisary vars
	Set tickers = UniqueRanges(ws, 1, "_")
	Dim forLoopMax As Double
	Dim wsRow As Double
	' Set vars
	forLoopMax = tickers.count - 1
	wsRow = 1
	' For each ticker, grab the nessisary data by appending to the 'string'
	For i = 0 To forLoopMax
		wsRow = wsRow + 1
		Dim ticker As String
		Dim tDate As String
		Dim tOpen As String
		Dim tClose As String
		Dim tVolume As String
		ticker = Replace(tickers(i), "_", "A")
		tDate = Replace(tickers(i), "_", "B")
		tOpen = Replace(tickers(i), "_", "C")
		tClose = Replace(tickers(i), "_", "F")
		tVolume = Replace(tickers(i), "_", "G")
		ws.Cells(wsRow,10).Formula = "=" & Left(ticker, InStr(ticker, ":") - 1)
		ws.Cells(wsRow, 11).Formula = "=INDEX(" & tOpen & ",MATCH(MIN(" & tDate & ")," & tDate & ",0))"
		ws.Cells(wsRow, 12).Formula = "=INDEX(" & tClose & ",MATCH(MAX(" & tDate & ")," & tDate & ",0))"
		ws.Cells(wsRow,13).Formula = "=" & CStr(ws.Cells(wsRow,12).Address()) & "-" & CStr(ws.Cells(wsRow,11).Address())
		ws.Cells(wsRow,13).FormatConditions.Delete
		Dim greaterThanZero As FormatCondition, lessThanZero As FormatCondition
		Set greaterThanZero = ws.Cells(wsRow,13).FormatConditions.Add(xlCellValue, xlGreater, "=0")
		Set lessThanZero = ws.Cells(wsRow,13).FormatConditions.Add(xlCellValue, xlLessEqual, "=0")
		With greaterThanZero
			.Interior.Color = vbGreen
		End With
		
		With lessThanZero
			.Interior.Color = vbRed
		End With
		' The percent change from opening price at the beginning 
		' of a given year to the closing price at the end of that year.
		'ws.Cells(wsRow,5).Value = ws.Cells(wsRow,4).Address()
		ws.Cells(wsRow,14).Formula = "=IFERROR(" & CStr(ws.Cells(wsRow,13).Address()) & "/" & CStr(ws.Cells(wsRow,11).Address()) & ",0)"
		ws.Cells(wsRow,14).NumberFormat="0.00%"
		' The total stock volume of the stock.
		ws.Cells(wsRow,15).Formula = "=SUM(" & tVolume & ")"
	Next i
End Function

Function UniqueRanges(ws As Worksheet, column As Double, splitValue As String) As Object
	' Create all the nessisary vars
	Dim rowsCount As Double
	Dim tickers As Object
	Dim count As Double
	Dim currentRange As String
	' Set vars
	rowsCount = ws.Cells(Rows.count, column).End(xlUp).Row
	Set tickers = CreateObject("System.Collections.ArrayList")
	count = 0
	' Create start value for array
	currentRange = "$" & splitValue & "$2:$" & splitValue & "$"
	For i = 3 To rowsCount - 1
		' for each row, if the next row is diffrent ->
		If Not ws.Cells(i, column) = ws.Cells(i + 1, column) Then
			' -> write the current index as the closing value
			currentRange = currentRange & CStr(i)
			tickers.Add currentRange
			' -> create the new starting range value
			currentRange = "$" & splitValue & "$" & CStr(i + 1) & ":$" & splitValue & "$"
			count = count + 1
		End If
    Next i
    ' add the closing value
    currentRange = currentRange & CStr(rowsCount)
    tickers.Add currentRange
    ' set and return
    Set UniqueRanges = tickers
End Function

Function BonusContent(ws As Worksheet) 
    ' Create all the nessisary vars
    Dim rowsCount As Double
    Dim tickers As String
    Dim percentChange As String
    Dim totalStock As String
	' Set vars
    rowsCount = ws.Range("J1", ws.Range("J1").End(xlDown)).Rows.Count
    tickers = ws.Range(ws.Cells(2,10), ws.Cells(rowsCount, 10)).Address()
    percentChange = ws.Range(ws.Cells(2,14), ws.Cells(rowsCount,14)).Address()
    totalStock = ws.Range(ws.Cells(2,15), ws.Cells(rowsCount,15)).Address()
    ' Ticker
    ws.Cells(2,19).Formula = "=INDEX(" & tickers & ",MATCH(T2," & percentChange & ",0))"
    ws.Cells(3,19).Formula = "=INDEX(" & tickers & ",MATCH(T3," & percentChange & ",0))"
    ws.Cells(4,19).Formula = "=INDEX(" & tickers & ",MATCH(T4," & totalStock & ",0))"
    ' compare
    ws.Cells(2,20).Formula = "=MAX(" & percentChange & ")"
    ws.Cells(3,20).Formula = "=MIN(" & percentChange & ")"
	ws.Cells(4,20).Formula = "=MAX(" & totalStock & ")"  
End Function

Function FormatwsSheet(ws As Worksheet)
	ws.Range("H:T").ClearContents 
	ws.Range("H:T").Clearformats 
	' Format ws Titles
	ws.Cells(1, 10).Value = "Ticker"
	ws.Cells(1, 11).Value = "Open Yearly Value"
	ws.Cells(1, 12).Value = "Closing Yearly Value"
	' Yearly change from opening price at the beginning of a given year to the closing price at the end of that year.
	ws.Cells(1, 13).Value = "Yearly Change"
	' The percent change from opening price at the beginning of a given year to the closing price at the end of that year.
	ws.Cells(1, 14).Value = "Percent Change"
	' The total stock volume of the stock.
	ws.Cells(1, 15).Value = "Total Stock"
	' Set BonusContent
	ws.Cells(2,18).Value = "Greatest % Increase"
	ws.Cells(3,18).Value = "Greatest % Decrease"
	ws.Cells(4,18).Value = "Greatest Total Volume"
	ws.Cells(1,19).Value = "Ticker"
	ws.Cells(1,20).Value = "Value"
	ws.Cells(2,20).NumberFormat="0.00%"
	ws.Cells(3,20).NumberFormat="0.00%"
End Function

Function FindOrCreateSheet(wSheetName As String) As Worksheet
    Dim wSheet As Worksheet
    On Error Resume Next
    Set wSheet = ThisWorkbook.Sheets(wSheetName)
    On Error GoTo 0
    If wSheet Is Nothing Then
        Dim count As Integer
        count = ThisWorkbook.Sheets.count
        Set wSheet = ThisWorkbook.Sheets.Add(After:=ThisWorkbook.Sheets(count))
        wSheet.Name = wSheetName
    End If
    Set FindOrCreateSheet = wSheet
End Function