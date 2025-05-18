// components/tiptap-ui/SlashMenu.tsx
import React from 'react'

interface SlashMenuProps {
  position: { top: number; left: number }
  onSelect: (type: string) => void
}

export const SlashMenu: React.FC<SlashMenuProps> = ({ position, onSelect }) => {
  const items = [
    { type: 'heading', label: 'Heading' },
    { type: 'paragraph', label: 'Paragraph' },
  ]

  return (
    <div
      className="absolute bg-white border shadow-lg rounded-md p-2 z-50"
      style={{ top: position.top, left: position.left }}
    >
      {items.map(item => (
        <button
          key={item.type}
          onClick={() => onSelect(item.type)}
          className="block px-4 py-2 hover:bg-gray-100 text-left w-full"
        >
          {item.label}
        </button>
      ))}
    </div>
  )
}
