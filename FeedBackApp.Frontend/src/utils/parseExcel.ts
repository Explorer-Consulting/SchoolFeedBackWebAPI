import * as XLSX from "xlsx";
import type { DropDownItem } from "@/models/StudentContext";

function parseDropDownItems(
  cell: unknown,
  question: string
): DropDownItem[] {
  const text = String(cell ?? "").trim();

  if (!text) {
    throw new Error(
      `A dropdown kérdéshez nincsenek lehetőségek: "${question}".`
    );
  }

  const items = text.split(";").map((entry): DropDownItem => {
    const separatorIndex = entry.indexOf("=");

    if (separatorIndex === -1) {
      throw new Error(
        `Hibás dropdown elem: "${entry}" a(z) "${question}" kérdésnél. ` +
        `Elvárt formátum: azonosító=felirat.`
      );
    }

    const id = entry.slice(0, separatorIndex).trim();
    const label = entry.slice(separatorIndex + 1).trim();

    if (!id || !label) {
      throw new Error(
        `A dropdown elem azonosítója és felirata is kötelező: "${question}".`
      );
    }

    return { id, label };
  });

  const uniqueIds = new Set(items.map((item) => item.id));

  if (uniqueIds.size !== items.length) {
    throw new Error(
      `Ismétlődő dropdown-azonosító a(z) "${question}" kérdésnél.`
    );
  }

  return items;
}

export function parseExcel(file: File, startDate: string, endDate: string, title: string) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: "array" });

        // --- StudentSets: every sheet that is not in the reserved array
        const reserved = ["questionnaireTemplate", "teachers", "questionnaireCreationParams", "sheetList"];
        const studentSets = workbook.SheetNames
          .filter((name) => !reserved.includes(name))
          .map((setId) => {
            const sheet = workbook.Sheets[setId];
            const rows = XLSX.utils.sheet_to_json<any>(sheet);
            return {
              setId,
              studentEmails: rows.map((row) => row.studentEmails),
            };
          });

        // --- questionnaireTemplate
        const templateSheet = workbook.Sheets["questionnaireTemplate"];
        const rawTemplate = XLSX.utils.sheet_to_json<any>(templateSheet);

        const questionnaireTemplate = rawTemplate.map((row) => {
          const isDropDown = row.type === "DropDown";

          // answerOptions
          const answerOptions = !isDropDown && row.answerOptions
            ? row.answerOptions.split(";").map((o: string) => o.trim())
            : undefined;

          // dropDownItems
          const dropDownItems = isDropDown
            ? parseDropDownItems(
                row.answerOptions,
                String(row.question ?? "")
              )
            : undefined;

          // dependency "19={1,2}"
          let dependency;
          if (row.dependency) {
            const match = row.dependency.match(/^(\d+)=\{(.*)\}$/);
            if (match) {
              dependency = {
                id: `q${match[1] - 1}`,
                answerConditions: match[2].split(";").map((o: string) => o.trim()),
              };
            }
          }
          
          return {
            question: row.question,
            type: row.type,
            category: row.category ? String(row.category) : "",  
            ...(row.description ? { description: String(row.description) } : {}),
            ...(answerOptions ? { answerOptions } : {}),
            ...(dropDownItems ? { dropDownItems } : {}),
            ...(dependency ? { dependency } : {}),
          };
        });

        // --- teachers
        const teachersSheet = workbook.Sheets["teachers"];
        const rawTeachers = XLSX.utils.sheet_to_json<any>(teachersSheet);
        const teachers = rawTeachers.map((row) => ({
          email: row.email,
          name: row.name,
        }));

        // --- questionnaireCreationParams
        const qcpSheet = workbook.Sheets["questionnaireCreationParams"];
        const rawQCP = XLSX.utils.sheet_to_json<any[]>(qcpSheet, { header: 1 }); // minden sor tömbként

        const questionnaireCreationParams = rawQCP
          .slice(1) 
          .filter((row) => {
            // in the first 2 cols. has to be a value
            return (row[0] && row[0].toString().trim() !== "") || (row[1] && row[1].toString().trim() !== "") || row.slice(2).some((cell) => cell && cell.toString().trim() !== "");
          })
          .map((row) => ({
            teacherEmail: row[0] || "",
            subjectName: row[1] || "",
            studentSetIds: row
              .slice(2)       
              .filter((cell) => cell && cell.toString().trim() !== ""), 
          }));


        const payload = {
          startDate,
          endDate,
          title,
          studentSets,
          questionnaireTemplate,
          teachers,
          questionnaireCreationParams,
        };

        resolve(payload);
      } catch (err) {
        reject(err);
      }
    };
    reader.readAsArrayBuffer(file);
  });
}
