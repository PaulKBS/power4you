interface StatusCardProps {
  title: string;
  value: string;
  icon: string;
  iconColor: string;
}

export default function StatusCard({ title, value, icon, iconColor }: StatusCardProps) {
  return (
    <div className="bg-card text-card-foreground p-4 rounded-lg shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-muted-foreground text-sm">{title}</p>
          <p className="text-2xl font-bold">{value}</p>
        </div>
        <div className={iconColor}>
          <i className={`fas ${icon} text-3xl`}></i>
        </div>
      </div>
    </div>
  );
} 