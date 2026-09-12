import { Download } from "lucide-react";
import { apiClient } from "../api/client";

/**
 * Three export buttons (CSV, Excel, PDF) wired to a module's
 * "{basePath}/export/{format}" endpoints - the same shape every controller
 * in the backend now exposes (CsvExporter / ExcelExporter / PdfTableExporter).
 * Centralized here so every module downloads the same way (blob + auth
 * header) instead of each page re-implementing it slightly differently -
 * that drift is exactly how InvoicesPage ended up calling the wrong
 * endpoint for years without anyone noticing.
 */
export default function ExportButtons({
  basePath, filenameBase, params, formats = ["csv", "excel", "pdf"]
}: {
  basePath: string;
  filenameBase: string;
  params?: Record<string, string | undefined>;
  formats?: Array<"csv" | "excel" | "pdf">;
}) {
  async function download(format: "csv" | "excel" | "pdf") {
    const query = params
      ? Object.entries(params)
          .filter(([, v]) => v !== undefined && v !== "")
          .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v as string)}`)
          .join("&")
      : "";
    const url = `${basePath}/export/${format}${query ? `?${query}` : ""}`;
    const response = await apiClient.get(url, { responseType: "blob" });
    const extension = format === "excel" ? "xls" : format;
    const blobUrl = URL.createObjectURL(response.data as Blob);
    const link = document.createElement("a");
    link.href = blobUrl;
    link.download = `${filenameBase}.${extension}`;
    link.click();
    URL.revokeObjectURL(blobUrl);
  }

  const labels: Record<"csv" | "excel" | "pdf", string> = { csv: "CSV", excel: "Excel", pdf: "PDF" };

  return (
    <div className="flex gap-2">
      {formats.map((format) => (
        <button key={format} className="btn-secondary" onClick={() => download(format)}>
          <Download className="h-4 w-4" /> {labels[format]}
        </button>
      ))}
    </div>
  );
}
