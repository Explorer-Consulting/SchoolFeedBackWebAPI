import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { QRCodeSVG } from 'qrcode.react';
import { Copy, Trash2, ExternalLink, ChevronDown, ChevronUp, Printer } from 'lucide-react';
import { toast } from 'sonner';
import { selfSignInLinkStorage } from '@/utils/selfSignInLinkStorage';
import { SelfSignInLink } from '@/models/SelfSignInLink';
import QrCodeModal from '@/components/ui/qr-code-modal';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

export default function SavedSelfSignInLinks() {
  const [savedLinks, setSavedLinks] = useState<SelfSignInLink[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [qrModalOpen, setQrModalOpen] = useState(false);
  const [selectedLinkForModal, setSelectedLinkForModal] = useState<SelfSignInLink | null>(null);
  const [printLink, setPrintLink] = useState<SelfSignInLink | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSavedLinks(selfSignInLinkStorage.getAll());
    }
  }, [isOpen]);

  useEffect(() => {
    setSavedLinks(selfSignInLinkStorage.getAll());
  }, []);

  const handleDeleteLink = (id: string) => {
    selfSignInLinkStorage.delete(id);
    setSavedLinks(selfSignInLinkStorage.getAll());
    toast.success('Link törölve');
  };

  const handleCopyUrl = (url: string) => {
    navigator.clipboard.writeText(url);
    toast.success('Link vágólapra másolva!');
  };

  const handleQrClick = (link: SelfSignInLink) => {
    setSelectedLinkForModal(link);
    setQrModalOpen(true);
  };

  const handlePrintQR = (link: SelfSignInLink) => {
    setPrintLink(link);
    setTimeout(() => {
      window.print();
      setPrintLink(null);
    }, 100);
  };

  const formatDate = (isoString: string) => {
    return new Date(isoString).toLocaleString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getExpirationLabel = (minutes: number) => {
    const years = minutes / (365 * 24 * 60);
    if (years >= 1) {
      return `${Math.round(years)} év`;
    }
    const days = minutes / (24 * 60);
    if (days >= 1) {
      return `${Math.round(days)} nap`;
    }
    const hours = minutes / 60;
    if (hours >= 1) {
      return `${Math.round(hours)} óra`;
    }
    return `${minutes} perc`;
  };

  return (
    <div className="mt-6">
      <Collapsible open={isOpen} onOpenChange={setIsOpen} className="w-full">
        <Card className="border rounded-lg shadow-sm">
          <CollapsibleTrigger asChild>
            <button className="flex w-full items-center justify-between px-4 py-2 text-left hover:bg-muted/50 transition-colors rounded">
              <div className="flex items-center gap-3">
                <span className="text-base font-semibold">
                    Self Sign-In Linkek
                </span>
                <span className="text-sm text-muted-foreground bg-muted px-2 py-0.5 rounded-full">
                  {savedLinks.length}
                </span>
              </div>
              {isOpen ? (
                <ChevronUp className="h-5 w-5 text-muted-foreground" />
              ) : (
                <ChevronDown className="h-5 w-5 text-muted-foreground" />
              )}
            </button>
          </CollapsibleTrigger>

          <CollapsibleContent>
            <CardContent className="pt-2 pb-4">
              {savedLinks.length === 0 ? (
                <div className="text-center py-12 text-muted-foreground">
                  <p className="text-sm">
                    Még nincsenek mentett linkek. Generálj egyet a fenti gombbal!
                  </p>
                </div>
              ) : (
                <div className="space-y-3">
                  {savedLinks.map((link) => (
                    <div
                      key={link.id}
                      className="border rounded-lg bg-card hover:bg-muted/30 transition-colors"
                    >
                      <div className="p-4">
                        <div className="flex flex-col lg:flex-row gap-4">
                          {/* QR Code + Title Section */}
                          <div className="flex gap-4 flex-1 min-w-0">
                            {/* QR Code Preview - Clickable */}
                            <div className="flex-shrink-0">
                              <button
                                onClick={() => handleQrClick(link)}
                                className="bg-white p-2 rounded border shadow-sm hover:shadow-md hover:scale-105 transition-all cursor-pointer"
                                title="Kattints a QR kód nagyításához"
                              >
                                <QRCodeSVG 
                                  id={`qr-${link.id}`}
                                  value={link.url} 
                                  size={100} 
                                  level="H" 
                                />
                              </button>
                            </div>

                            {/* Link Details */}
                            <div className="flex-1 min-w-0 space-y-2">
                              <h3 className="font-semibold text-base">
                                {link.templateTitle}
                              </h3>
                              
                              <div className="space-y-1 text-xs text-muted-foreground">
                                <div>
                                  <span className="font-medium">Létrehozva:</span>{' '}
                                  <span className="text-foreground/80">
                                    {formatDate(link.createdAt)}
                                  </span>
                                </div>
                                <div>
                                  <span className="font-medium">Lejárat:</span>{' '}
                                  <span className="text-foreground/80">
                                    {formatDate(link.expiresAt)}
                                  </span>
                                </div>
                                <div>
                                  <span className="font-medium">Érvényesség:</span>{' '}
                                  <span className="text-foreground/80">
                                    {getExpirationLabel(link.expirationMinutes)}
                                  </span>
                                </div>
                              </div>

                              <div className="pt-1">
                                <Input
                                  value={link.url}
                                  readOnly
                                  className="h-8 text-xs font-mono bg-muted/50 border-muted-foreground/20"
                                />
                              </div>
                            </div>
                          </div>

                          {/* Action Buttons - Vertical Stack on Right */}
                          <div className="flex lg:flex-col gap-2 lg:w-32 flex-shrink-0">
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handleCopyUrl(link.url)}
                              className="flex-1 lg:w-full h-9"
                            >
                              <Copy className="h-4 w-4 lg:mr-2" />
                              <span className="hidden lg:inline text-xs">Másolás</span>
                            </Button>

                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => handlePrintQR(link)}
                              className="flex-1 lg:w-full h-9"
                            >
                              <Printer className="h-4 w-4 lg:mr-2" />
                              <span className="hidden lg:inline text-xs">Nyomtatás</span>
                            </Button>

                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => window.open(link.url, '_blank')}
                              className="flex-1 lg:w-full h-9"
                            >
                              <ExternalLink className="h-4 w-4 lg:mr-2" />
                              <span className="hidden lg:inline text-xs">Nyitás</span>
                            </Button>

                            <Button
                              size="sm"
                              variant="destructive"
                              onClick={() => handleDeleteLink(link.id)}
                              className="flex-1 lg:w-full h-9"
                            >
                              <Trash2 className="h-4 w-4 lg:mr-2" />
                              <span className="hidden lg:inline text-xs">Törlés</span>
                            </Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </CollapsibleContent>
        </Card>
      </Collapsible>

      {/* QR Code Modal */}
      {selectedLinkForModal && (
        <QrCodeModal
          isOpen={qrModalOpen}
          onClose={() => setQrModalOpen(false)}
          url={selectedLinkForModal.url}
          title={selectedLinkForModal.templateTitle}
          showActions={false}  
          qrSize={300}         
        />
      )}

      {/* Print Template - Shown only when printing */}
        {printLink && (
        <>
            <style>{`
            @media screen {
                .print-qr-content {
                display: none !important;
                }
            }
            
            @media print {
                @page {
                size: A4 portrait;
                margin: 1cm;
                }
                
                * {
                visibility: hidden;
                }
                
                .print-qr-content,
                .print-qr-content * {
                visibility: visible !important;
                }
                
                .print-qr-content {
                position: absolute;
                left: 50%;
                top: 1.5cm;
                transform: translateX(-50%);
                width: 90%;
                max-width: 500px;
                }
            }
            `}</style>

            <div className="print-qr-content">
                <div style={{ textAlign: 'center'}}>
                    <h1 style={{ 
                        fontSize: '18px', 
                        marginBottom: '12px', 
                        color: '#333',
                        fontWeight: '600'
                    }}>
                        {printLink.templateTitle}
                    </h1>
                    
                    <div style={{ 
                        margin: '0 auto 12px auto',
                        padding: '12px',
                        background: 'white',
                        border: '2px solid #e5e7eb',
                        borderRadius: '8px',
                        display: 'inline-block'
                    }}>
                        <QRCodeSVG
                            value={printLink.url}
                            size={200}
                            level="H"
                        />
                    </div>

                    <div style={{
                        textAlign: 'left',
                        padding: '10px',
                        background: '#f9fafb',
                        borderRadius: '6px',
                        border: '1px solid #e5e7eb'
                    }}>
                        <div style={{ marginBottom: '6px', fontSize: '12px' }}>
                            <span style={{ 
                                fontWeight: 'bold', 
                                color: '#4b5563',
                                display: 'inline-block',
                                width: '100px'
                            }}>
                                Létrehozva:
                            </span>
                            <span style={{ color: '#1f2937' }}>
                                {formatDate(printLink.createdAt)}
                            </span>
                        </div>

                        <div style={{ marginBottom: '6px', fontSize: '12px' }}>
                            <span style={{ 
                                fontWeight: 'bold', 
                                color: '#4b5563',
                                display: 'inline-block',
                                width: '100px'
                            }}>
                                Lejárat:
                            </span>
                            <span style={{ color: '#1f2937' }}>
                                {formatDate(printLink.expiresAt)}
                            </span>
                        </div>

                        <div style={{ fontSize: '12px' }}>
                            <span style={{ 
                            fontWeight: 'bold', 
                            color: '#4b5563',
                            display: 'inline-block',
                            width: '100px'
                            }}>
                            Érvényesség:
                            </span>
                            <span style={{ color: '#1f2937' }}>
                            {getExpirationLabel(printLink.expirationMinutes)}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </>
        )}
    </div>
  );
}