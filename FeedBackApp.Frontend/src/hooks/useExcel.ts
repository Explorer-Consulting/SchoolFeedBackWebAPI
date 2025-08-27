import * as XLSX from "xlsx";

export function parseExcel(file: File, startDate: string, endDate: string, title: string) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: "array" });

        // --- StudentSets: minden sheet ami nem a fix 3 (template, teachers, qcp)
        const reserved = ["questionnaireTemplate", "teachers", "questionnaireCreationParams"];
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
        const questionnaireTemplate = rawTemplate.map((row) => ({
          question: row.question,
          type: row.type,
          ...(row.answerOptions
            ? { answerOptions: row.answerOptions.split(";").map((o: string) => o.trim()) }
            : {}),
        }));

        // --- teachers
        const teachersSheet = workbook.Sheets["teachers"];
        const rawTeachers = XLSX.utils.sheet_to_json<any>(teachersSheet);
        const teachers = rawTeachers.map((row) => ({
          email: row.email,
          name: row.name,
        }));

        // --- questionnaireCreationParams
        const qcpSheet = workbook.Sheets["questionnaireCreationParams"];
        const rawQCP = XLSX.utils.sheet_to_json<any>(qcpSheet);
        const questionnaireCreationParams = rawQCP.map((row) => ({
          teacherEmail: row.teacherEmail,
          subjectName: row.subjectName,
          studentSetIds: row.studentSetIds.split(";").map((s: string) => s.trim()),
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
