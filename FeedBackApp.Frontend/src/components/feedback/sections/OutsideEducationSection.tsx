import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

type OutsideEducationSectionProps = {
    q18: string;
    setQ18: (val: string) => void;
    q19: string;
    setQ19: (val: string) => void;
    q20: string[];
    setQ20: React.Dispatch<React.SetStateAction<string[]>>;
    q21: string[];
    setQ21: React.Dispatch<React.SetStateAction<string[]>>;
    q22: string;
    setQ22: (val: string) => void;
    q23: string;
    setQ23: (val: string) => void;
    isAttendingOutside: boolean;
    toggleMulti: (
        value: string,
        setFn: (updater: (prev: string[]) => string[]) => void
    ) => void;
}

export default function OutsideEducationSection({
    q18, setQ18,
    q19, setQ19,
    q20, setQ20,
    q21, setQ21,
    q22, setQ22,
    q23, setQ23,
    isAttendingOutside,
    toggleMulti
}: OutsideEducationSectionProps) {
    return (
        <section className="space-y-6">
            <header>
                <h2 className="text-xl font-semibold">Iskolán kívüli oktatás</h2>
            </header>

            <div className="space-y-2">
                <Label>18) A Tanár részesít külön foglalkozásban,, hogy felkészítsen vizsgára/versenyre/szereplésre:</Label>
                <RadioGroup value={["1", "2", "3"].includes(q18) ? q18 : ""} onValueChange={setQ18}>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q18-1" value="1" />
                        <Label htmlFor="q18-1">igen, elkéreztetve más Tanárok óráiról</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q18-2" value="2" />
                        <Label htmlFor="q18-2">igen, iskolaidőn kívül</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q18-3" value="3" />
                        <Label htmlFor="q18-3">nincs külön foglalkozás ebből a tantárgyból</Label>
                    </div>
                </RadioGroup>
                <Input
                    type="text"
                    placeholder="Egyéb, éspedig:"
                    value={!["1", "2", "3"].includes(q18) ? q18 : ""}
                    onChange={(e) => setQ18(e.target.value)}
                    className="placeholder:text-[13px]"
                />
            </div>


            <div className="space-y-2">
                <Label>19) Ebből a tantárgyból iskolán kívül:</Label>
                <RadioGroup value={q19} onValueChange={setQ19}>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q19-1" value="1" />
                        <Label htmlFor="q19-1">magánórára, egyéni felkészítőre járok</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q19-2" value="2" />
                        <Label htmlFor="q19-2">csoportos felkészülésen veszek részt</Label>
                    </div>
                    <div className="flex items-center space-x-2">
                        <RadioGroupItem id="q19-3" value="3" />
                        <Label htmlFor="q19-3"> nem veszek részt iskolán kívüli oktatásban ebből a tantárgyból</Label>
                    </div>
                </RadioGroup>
            </div>

            {isAttendingOutside && (
                <div className="space-y-2">
                    <Label>20) Az iskolán kívüli oktatáson azért veszek részt, mert:</Label>
                    {[
                        { id: "1", label: "nagyon tetszik a téma, el szeretnék mélyülni még jobban az ismeretekben" },
                        { id: "2", label: "szükségesnek érzem, mert nagyon le vagyok maradva az osztáytársakhoz képest" },
                        { id: "3", label: " úgy érzem, hogy az iskolai oktatás/felkészítés nem elég a vizsgák sikerességéhez/jó jegyek eléréséhez" },
                        { id: "4", label: "a szüleim ragaszkodnak hozzá, hogy magánórára járjak " },
                        { id: "5", label: "túl sok a szabadidőm, nincs mivel kitöltsem" },
                    ].map(opt => (
                        <div key={opt.id} className="flex items-center space-x-2">
                            <Checkbox
                                id={`q20-${opt.id}`}
                                checked={q20.includes(opt.id)}
                                onCheckedChange={() => toggleMulti(opt.id, setQ20)}
                            />
                            <Label htmlFor={`q20-${opt.id}`}>{opt.label}</Label>
                        </div>
                    ))}
                </div>
            )}

            <div className="space-y-2">
                <Label>21) Szeretném, ha ebből a tantárgyból:</Label>
                {[
                    { id: "1", label: " gyakorlati szempontok szerint is megközelítenénk órákon a tananyagot" },
                    { id: "2", label: "kevesebb házifeladat lenne" },
                    { id: "3", label: "kedvesebb/barátibb lenne a tanárunk" },
                    { id: "4", label: "több információt kapnék, ami felhasználhatnék a mindennapokban is" },
                    { id: "5", label: "Teljesen elégedett vagyok a mostani helyzettel" },
                ].map(opt => (
                    <div key={opt.id} className="flex items-center space-x-2">
                        <Checkbox
                            id={`q21-${opt.id}`}
                            checked={q21.includes(opt.id)}
                            onCheckedChange={() => toggleMulti(opt.id, setQ21)}
                        />
                        <Label htmlFor={`q21-${opt.id}`}>{opt.label}</Label>
                    </div>
                ))}
            </div>

            <div className="space-y-2">
                <Label htmlFor="q22"> 22) Kérünk, fogalmazd meg röviden, mi volt a legjobb ezen az órán? </Label>
                <Textarea id="q22"
                    value={q22}
                    onChange={(e) => setQ22(e.target.value)}
                    placeholder="Rövid válasz..."
                    maxLength={300}
                />
                <p className="text-sm text-gray-500">
                    {q22.length}/400 karakter (min. 50) </p>
            </div>

            <div className="space-y-2">
                <Label htmlFor="q23"> 23) Kérünk, fogalmazd meg röviden, mi nem tetszett ezen az órán? </Label>
                <Textarea id="q23"
                    value={q23}
                    onChange={(e) => setQ23(e.target.value)}
                    placeholder="Rövid válasz..."
                    maxLength={300}
                />
                <p className="text-sm text-gray-500">
                    {q23.length}/400 karakter (min. 50) </p>
            </div>
        </section>
    )
}