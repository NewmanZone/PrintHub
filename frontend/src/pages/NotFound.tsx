import React from 'react'
import { Link } from 'react-router-dom'
import { Compass } from 'lucide-react'
import { Button } from '../components/ui/Button'
import { EmptyState } from '../components/ui/EmptyState'

export const NotFound: React.FC = () => {
  return (
    <EmptyState
      icon={<Compass size={24} />}
      title="Page not found"
      description="This workspace view does not exist or has moved."
      action={<Link to="/dashboard"><Button>Back to dashboard</Button></Link>}
    />
  )
}
