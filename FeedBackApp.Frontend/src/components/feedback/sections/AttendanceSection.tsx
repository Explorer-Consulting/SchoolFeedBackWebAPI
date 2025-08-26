import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";

type AttendanceSectionProps = {
    q24: string;
    setQ24: (val: string) => void;
    q25: string;
    setQ25: (val: string) => void;
    q26: string;
    setQ26: (val: string) => void;
}

export default function AttendanceSection({
    q24, setQ24,
    q25, setQ25,
    q26, setQ26
}: AttendanceSectionProps) {
    return (
        <section className="space-y-6">
            <header>
                <h2 className="text-xl font-semibold">Jelenlét és elmaradt tanórák</h2>
            </header>

            <div className="space-y-2">
                <Label>24) Ebben a tanévben jelen voltam a tantárgyban megtartott:</Label>
                <RadioGroup value={q24} onValueChange={setQ24} className="grid gap-2 md:grid-cols-2">
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q24-25" value="1" />
                        <Label htmlFor="q24-25">órák 25%-án</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q24-50" value="2" />
                        <Label htmlFor="q24-50">órák 50%-án</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q24-75" value="3" />
                        <Label htmlFor="q24-75">órák 75%-án</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q24-90" value="4" />
                        <Label htmlFor="q24-90">órák több mint 90%-án</Label>
                    </div>
                </RadioGroup>
            </div>

            <div className="space-y-2">
                <Label>25) Válaszd ki a tantárgyra vonatkozó helyes megállapítást:</Label>
                <RadioGroup value={q25} onValueChange={setQ25} className="grid gap-2 md:grid-cols-2">
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q25-25" value="1" />
                        <Label htmlFor="q25-25">az órák legfeljebb 25%-a volt megtartva</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q25-50" value="2" />
                        <Label htmlFor="q25-50">az órák legfeljebb 50%-a volt megtartva</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q25-75" value="3" />
                        <Label htmlFor="q25-75">az órák legfeljebb 75%-a volt megtartva</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q25-90" value="4" />
                        <Label htmlFor="q25-90">az órák legalább 90%-a meg volt tartva</Label>
                    </div>
                </RadioGroup>
            </div>

            <div className="space-y-2">
                <Label>26) Válaszd ki a gyakoribb megállapítást arra az esetre, ha a Tanárod nem tudta megtartani az órát:</Label>
                <RadioGroup value={q26} onValueChange={setQ26} className="grid gap-2 md:grid-cols-2">
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q26-helyettes" value="1" />
                        <Label htmlFor="q26-helyettes">volt helyettesítő tanár</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q26-lyukas" value="2" />
                        <Label htmlFor="q26-lyukas">nem volt helyettesítés, lyukas óra lett belőle</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q26-atren" value="3" />
                        <Label htmlFor="q26-atren">átrendeződött az órarend, így egy órával később/hamarabb mentünk/jöttünk az iskolából</Label>
                    </div>
                </RadioGroup>
            </div>
        </section>
    )
}