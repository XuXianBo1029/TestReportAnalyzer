\# Test Report Analyzer



\## 專案簡介



Test Report Analyzer 是一個使用 C# WinForms 開發的測試報告分析工具，可匯入 CSV 測試資料，顯示測試結果表格，並統計 PASS / FAIL 數量、良率與 ErrorCode 分布，協助快速整理測試結果與產出摘要報告。



\## 使用技術



\* C#

\* Windows Forms

\* DataGridView

\* CSV File I/O

\* Dictionary 統計資料



\## 功能



\* 匯入 CSV 測試報告

\* 顯示測試資料表格

\* 統計總筆數

\* 統計 PASS / FAIL 數量

\* 計算良率 Yield

\* 統計 ErrorCode 出現次數

\* 匯出摘要 CSV 報告



\## CSV 格式範例



```csv

Time,Station,Item,Result,ErrorCode

2000-01-23 10:00:01,A01,VoltageTest,PASS,

2000-01-23 10:00:03,A01,CurrentTest,FAIL,E001

2000-01-23 10:00:05,A02,SignalTest,PASS,

2000-01-23 10:00:07,A02,BootTest,FAIL,E002

2000-01-23 10:00:09,A03,NetworkTest,PASS,

2000-01-23 10:00:11,A03,TemperatureTest,FAIL,E001

2000-01-23 10:00:13,A04,StorageTest,PASS,

2000-01-23 10:00:15,A04,MemoryTest,PASS,

```



\## 匯出摘要格式



```csv

Category,Name,Value

Summary,Total,8

Summary,PASS,5

Summary,FAIL,3

Summary,Yield,62.50%

ErrorCode,E001,2

ErrorCode,E002,1

```



\## 目前限制



目前支援簡單 CSV 格式，欄位內容不包含逗號或特殊引號。後續可擴充為支援更完整的 CSV 解析、條件篩選、圖表顯示與資料庫儲存。



\## 執行畫面



!\[Main UI](./docs/screenshot\_main.png)





