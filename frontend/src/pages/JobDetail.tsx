import React from 'react'
import { useParams } from 'react-router-dom'
import { ErrorState } from '../components/ui/ErrorState'

export const JobDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>()
  return <ErrorState title="Job tracking is not live yet" message={`No live job data is available for ${id ?? 'this job'}.`} />
}
