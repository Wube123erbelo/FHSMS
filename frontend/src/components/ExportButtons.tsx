import { useState, type ReactNode } from "react";
import { Download, FileSpreadsheet, FileText, Table } from "lucide-react";
import DropdownButton from "./DropdownButton";
import { apiClient } from "../api/client";
import { useTranslation } from "../i18n/LanguageContext";

/**
 * One "Download" button that expands into CSV / Excel / PDF options, wired
 * to a module's "{basePath}/export/{format}" endpoints - the same shape
 * every controller in the backend exposes (CsvExporter / ExcelExporter /
 * PdfTableExporter). Centralized here so every module downloads the same
 * way (blob + auth header) instead of each page re-implementing it slightly
 * differently, and so every page presents one tidy button instead of three
 * separate ones cluttering the header.
 */
export default function ExportButtons({
  basePath, filenameBase, params, formats = ["csv", "excel", "pdf"]
}: {
  basePath: string;
  filenameBase: string;
  params?: Record<string, string | undefined>;
  formats?: Array<"csv" | "excel" | "pdf">;
}) {
  const { t } = useTranslation();
  const [downloading, setDownloading] = useState<"csv" | "excel" | "pdf" | null>(null);

  async function download(format: "csv" | "excel" | "pdf") {
    setDownloading(format);
    try {
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
    } finally {
      setDownloading(null);
    }
  }

  const labels: Record<"csv" | "excel" | "pdf", string> = { csv: "CSV", excel: "Excel", pdf: "PDF" };
  const icons: Record<"csv" | "excel" | "pdf", ReactNode> = {
    csv: <Table className="h-4 w-4" />,
    excel: <FileSpreadsheet className="h-4 w-4" />,
    pdf: <FileText className="h-4 w-4" />
  };

  return (
    <DropdownButton
      label={downloading ? `${t("common.downloading")}...` : t("common.download")}
      icon={<Download className="h-4 w-4" />}
      variant="secondary"
      align="right"
      disabled={downloading !== null}
      options={formats.map((format) => ({
        key: format,
        label: labels[format],
        icon: icons[format],
        onClick: () => download(format),
        disabled: downloading !== null
      }))}
    />
  );
}
