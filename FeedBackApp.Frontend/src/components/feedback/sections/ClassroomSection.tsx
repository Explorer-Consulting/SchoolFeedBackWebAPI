import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";

type Props = {
  qValues: string[];
  setQValues: ((v: string) => void)[];
  likertValues: string[];
};

export default function ClassroomSection({ qValues, setQValues, likertValues }: Props) {
  const questions = [
    "1) A Tanár érthetően magyarázza a tananyagot.",
    "2) A Tanár olyan magyarázatokat ad, amelyek segítenek a hatékony tanulásban.",
    "3) A tanórai feladatok érdekesek.",
    "4) A Tanár bátorítja és bevonja a teljes osztályközösséget a tanórák tevékenységébe.",
    "5) A Tanár szorgalmazza a diákok közötti együttműködést.",
    "6) A Tanár motivál, hogy ezeken az órákon részt vegyek.",
    "7) A Tanár digitális eszközöket használ a tanításhoz.",
    "8) Kielégítő tájékoztatást kapok a képességeim felmérésének módjairól, illetve ismereteim felmérésének kritériumairól.",
    "9) A Tanár támogatja a tanulók közti versenyszellemet.",
    "10) A tanulókat bátorítja gondolataik kifejezésére és a véleményalkotásra.",
    "11) Az órákon kellemes a hangulat.",
    "12) Biztonságban és komfortosan érzem magam az órákon.",
    "13) A tananyag elsajátításának üteme számomra:",
    "14) A Tanárt érdekli, hogy én jól érezzem magam az órákon.",
    "15) Nagy erőfeszítésembe kerül az otthoni felkészülés, hogy ebből a tárgyból jó eredményeket érjek el.",
    "16) A Tanár figyelembe veszi és betartja a Tanulók Statútumát.",
    "17) Ezen az órán ideges vagyok, gyomoridegem van."
  ];

  return (
    <section className="space-y-6">
      <header>
        <h2 className="text-xl font-semibold">Osztálytermi tevékenység</h2>
        <p className="text-sm text-muted-foreground">
          1 = egyáltalán nem értek egyet, 5 = teljes mértékben egyetértek
        </p>
      </header>

      <div className="space-y-5">
        {questions.map((text, i) => (
          <div key={i} className="space-y-2">
            <Label>{text}</Label>
            <RadioGroup
              value={qValues[i]}
              onValueChange={setQValues[i]}
              className="grid grid-cols-5 gap-2"
            >
              {likertValues.map((v) => (
                <div key={v} className="flex items-center space-x-2">
                  <RadioGroupItem id={`q${i + 1}-${v}`} value={v} />
                  <Label htmlFor={`q${i + 1}-${v}`}>{v}</Label>
                </div>
              ))}
            </RadioGroup>
            {i === 12 && (
              <p className="text-xs text-muted-foreground">
                1 = nagyon lassú, 2 = lassú, 3 = megfelelő, 4 = gyors, 5 = nagyon gyors
              </p>
            )}
          </div>
        ))}
      </div>
    </section>
  );
}