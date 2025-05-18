import "@blocknote/core/fonts/inter.css";
import { BlockNoteView } from "@blocknote/mantine";
import "@blocknote/mantine/style.css";
import { useCreateBlockNote } from "@blocknote/react";

export default function HomePage() {
  const editor = useCreateBlockNote();

  return (
    <div className="flex justify-center px-4 py-8">
      <div className="w-full max-w-3xl">
        <BlockNoteView editor={editor} />
      </div>
    </div>
  );
}
