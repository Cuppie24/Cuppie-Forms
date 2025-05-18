import { Extension } from '@tiptap/core'

export const SlashCommand = Extension.create({
  name: 'slash-command',

  addOptions() {
    return {
      onSlash: null as ((pos: number) => void) | null,
    }
  },

  addKeyboardShortcuts() {
    return {
      '/': () => {
        const { from } = this.editor.state.selection
        this.options.onSlash?.(from)
        return true // предотвращает ввод "/"
      },
    }
  },
})