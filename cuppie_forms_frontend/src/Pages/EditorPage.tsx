import "@blocknote/core/fonts/inter.css";
import { BlockNoteView } from "@blocknote/mantine";
import "@blocknote/mantine/style.css";
import { useCreateBlockNote } from "@blocknote/react";
import apiAxios from "@/api/axios";
import { useState } from "react";

export default function HomePage() {
  const editor = useCreateBlockNote();
  const [savedContent, setSavedContent] = useState<string | null>(null);

  const handleSave = async () => {
    if (!editor) return;
    try {
      const blocks = editor.document;
      const content = JSON.stringify(blocks);
      const result = await apiAxios.post("pages/save", {content});

      // Предполагаем, что API возвращает JSON обратно в поле result.data
      setSavedContent(JSON.stringify(result.data, null, 2));
      alert("Содержимое сохранено!");
    } catch (error) {
      console.error("Ошибка при сохранении:", error);
      alert("Не удалось сохранить содержимое.");
    }
  };

  return (
    <div className="flex flex-col items-center px-4 py-8">
      <div className="w-full max-w-6xl mb-4 flex justify-end">
        <button
          onClick={handleSave}
          disabled={!editor}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          Сохранить
        </button>
      </div>

      <div className="w-full max-w-6xl flex gap-8">
        {/* Редактор */}
        <div className="w-1/2 border rounded p-4 shadow">
          <BlockNoteView editor={editor} />
        </div>

        {/* JSON-отображение */}
        <div className="w-1/2 border rounded p-4 bg-gray-100 overflow-auto text-sm whitespace-pre-wrap font-mono shadow text-gray-800">
          {savedContent ? (
            <pre>{savedContent}</pre>
          ) : (
            <p className="text-gray-500">Сохранённый JSON появится здесь...</p>
          )}
        </div>
      </div>
    </div>
  );
}
