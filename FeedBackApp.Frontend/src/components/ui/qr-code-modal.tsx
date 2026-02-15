import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { QRCodeSVG } from "qrcode.react";
import { Copy, Download, Printer } from "lucide-react";
import { toast } from "sonner";

type QrCodeModalProps = {
  isOpen: boolean;
  onClose: () => void;
  url: string;
  title: string;
  expiresAt?: string;
  showActions?: boolean;  
  qrSize?: number;        
}

export default function QrCodeModal({ 
  isOpen, 
  onClose, 
  url, 
  title, 
  expiresAt,
  showActions = true,  
  qrSize = 256         
}: QrCodeModalProps) {
  
  const handleCopyLink = () => {
    navigator.clipboard.writeText(url);
    toast.success("Link vágólapra másolva!");
  };

  const handleDownloadQR = () => {
    const svg = document.getElementById("qr-code-svg");
    if (!svg) return;

    const svgData = new XMLSerializer().serializeToString(svg);
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");
    const img = new Image();

    img.onload = () => {
      canvas.width = img.width;
      canvas.height = img.height;
      ctx?.drawImage(img, 0, 0);
      const pngFile = canvas.toDataURL("image/png");

      const downloadLink = document.createElement("a");
      downloadLink.download = `qr-code-${title}.png`;
      downloadLink.href = pngFile;
      downloadLink.click();
      
      toast.success("QR kód letöltve!");
    };

    img.src = "data:image/svg+xml;base64," + btoa(svgData);
  };

  const handlePrint = () => {
    window.print();
    toast.success("Nyomtatási ablak megnyitva!");
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col items-center space-y-4 py-4">
          {/* QR Code */}
          <div className="bg-white p-4 rounded-lg border shadow-sm">
            <QRCodeSVG
              id="qr-code-svg"
              value={url}
              size={qrSize}
              level="H"
              includeMargin={true}
            />
          </div>

          {/* Conditionally render actions section */}
          {showActions && (
            <>
              {/* Link display */}
              <div className="w-full">
                <p className="text-sm text-muted-foreground mb-2">Megosztható link:</p>
                <div className="flex items-center gap-2">
                  <input
                    type="text"
                    value={url}
                    readOnly
                    className="flex-1 p-2 border rounded text-sm"
                  />
                  <Button size="sm" variant="outline" onClick={handleCopyLink}>
                    <Copy className="h-4 w-4" />
                  </Button>
                </div>
              </div>

              {/* Expiration info */}
              {expiresAt && (
                <p className="text-xs text-muted-foreground">
                  Lejárat: {new Date(expiresAt).toLocaleString("hu-HU")}
                </p>
              )}

              {/* Action buttons */}
              <div className="flex gap-2 w-full">
                <Button onClick={handleDownloadQR} variant="outline" className="flex-1">
                  <Download className="h-4 w-4 mr-2" />
                  Letöltés
                </Button>
                <Button onClick={handlePrint} variant="outline" className="flex-1">
                  <Printer className="h-4 w-4 mr-2" />
                  Nyomtatás
                </Button>
              </div>
            </>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}